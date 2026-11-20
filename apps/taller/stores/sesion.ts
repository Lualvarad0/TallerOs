import { defineStore } from 'pinia';
import type { EmpresaAccesible } from '@talleros/api-client';
import { decodificarJwt, tokenExpirado, type ClaimsSesion } from '~/utils/jwt';

const CLAVE_SESION = 'talleros-token-sesion';
const CLAVE_EMAIL = 'talleros-email-sesion';

function leerTokenGuardado(): string | null {
  try {
    return sessionStorage.getItem(CLAVE_SESION);
  } catch {
    return null;
  }
}

function leerEmailGuardado(): string | null {
  try {
    return sessionStorage.getItem(CLAVE_EMAIL);
  } catch {
    return null;
  }
}

export const useSesionStore = defineStore('sesion', () => {
  const tokenSesion = ref<string | null>(null);
  const tokenPreAuth = ref<string | null>(null);
  const empresasDisponibles = ref<EmpresaAccesible[]>([]);
  // El JWT no trae el correo (ver ClaimsSesion): se guarda el que la persona tecleó al entrar, solo
  // para mostrarlo en la UI (nunca para autorizar nada).
  const email = ref<string | null>(null);

  const claims = computed<ClaimsSesion | null>(() => (tokenSesion.value ? decodificarJwt(tokenSesion.value) : null));

  const estaAutenticado = computed(() => !!claims.value && !tokenExpirado(claims.value));

  /** El token que debe mandar el plugin de fetch en Authorization, según en qué paso del login está. */
  const tokenParaPeticiones = computed(() => tokenSesion.value ?? tokenPreAuth.value);

  function restaurar() {
    const guardado = leerTokenGuardado();
    if (guardado && !tokenExpirado(decodificarJwt(guardado))) {
      tokenSesion.value = guardado;
      email.value = leerEmailGuardado();
    }
  }

  function guardarTokenSesion(token: string) {
    tokenSesion.value = token;
    tokenPreAuth.value = null;
    empresasDisponibles.value = [];
    try {
      sessionStorage.setItem(CLAVE_SESION, token);
    } catch {
      /* la sesión sigue en memoria */
    }
  }

  /** Resultado de iniciarSesion: o ya hay token de sesión (super admin / una sola empresa), o hay
   * que elegir empresa (varias empresas, token pre-auth). */
  function procesarLogin(resultado: { tokenSesion: string | null; tokenPreAutenticacion: string | null; empresas: EmpresaAccesible[] }, emailIngresado: string) {
    email.value = emailIngresado;
    try {
      sessionStorage.setItem(CLAVE_EMAIL, emailIngresado);
    } catch {
      /* se mantiene en memoria */
    }

    if (resultado.tokenSesion) {
      guardarTokenSesion(resultado.tokenSesion);
      return;
    }
    tokenPreAuth.value = resultado.tokenPreAutenticacion;
    empresasDisponibles.value = resultado.empresas;
  }

  function cerrarSesion() {
    tokenSesion.value = null;
    tokenPreAuth.value = null;
    empresasDisponibles.value = [];
    email.value = null;
    try {
      sessionStorage.removeItem(CLAVE_SESION);
      sessionStorage.removeItem(CLAVE_EMAIL);
    } catch {
      /* nada que limpiar */
    }
  }

  return { tokenSesion, tokenPreAuth, empresasDisponibles, email, claims, estaAutenticado, tokenParaPeticiones, restaurar, guardarTokenSesion, procesarLogin, cerrarSesion };
});
