using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using TallerOS.Application.Comun.Excepciones;

namespace TallerOS.Api.Middleware;

/// <summary>Último punto de la Api que ve las excepciones de Application/Infrastructure: las
/// traduce a un código HTTP + ProblemDetails en vez de un 500 crudo. 23505 es el SqlState de
/// Postgres para violación de índice único (ej. RUC/email duplicado que ningún handler chequeó
/// de antemano).</summary>
public sealed class ManejoDeExcepcionesMiddleware(RequestDelegate siguiente, ILogger<ManejoDeExcepcionesMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await siguiente(context);
        }
        catch (Exception ex)
        {
            await EscribirRespuesta(context, ex);
        }
    }

    private async Task EscribirRespuesta(HttpContext context, Exception ex)
    {
        var problema = Clasificar(ex);
        var status = problema.Status ?? StatusCodes.Status500InternalServerError;
        if (status == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(ex, "Error no controlado en {Path}", context.Request.Path);
        }

        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(problema);
    }

    private static ProblemDetails Clasificar(Exception ex) => ex switch
    {
        EntidadNoEncontradaException => NuevoProblema(StatusCodes.Status404NotFound, "No encontrado", ex.Message),
        ConflictoDeNegocioException => NuevoProblema(StatusCodes.Status409Conflict, "Conflicto", ex.Message),
        SinContextoDeEmpresaException => NuevoProblema(StatusCodes.Status403Forbidden, "Sin empresa seleccionada", ex.Message),
        CredencialesInvalidasException => NuevoProblema(StatusCodes.Status401Unauthorized, "No autorizado", ex.Message),
        ValidationException validacion => ProblemaDeValidacion(validacion),
        // EF Core envuelve el error real de Postgres en DbUpdateException; el que trae el SqlState
        // es el InnerException (PostgresException), no la excepción de EF en sí.
        DbUpdateException { InnerException: PostgresException { SqlState: "23505" } } =>
            NuevoProblema(StatusCodes.Status409Conflict, "Conflicto", "Ya existe un registro con ese valor único."),
        _ => NuevoProblema(StatusCodes.Status500InternalServerError, "Error interno", "Ocurrió un error inesperado."),
    };

    private static ProblemDetails ProblemaDeValidacion(ValidationException ex)
    {
        var problema = NuevoProblema(StatusCodes.Status400BadRequest, "Solicitud inválida", ex.Message);
        problema.Extensions["errores"] = ex.Errors
            .Select(e => new { campo = e.PropertyName, mensaje = e.ErrorMessage })
            .ToList();
        return problema;
    }

    private static ProblemDetails NuevoProblema(int status, string titulo, string detalle) =>
        new() { Status = status, Title = titulo, Detail = detalle };
}
