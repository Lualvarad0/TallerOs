import type { ClienteApi } from '@talleros/api-client';

export function useApi(): ClienteApi {
  return useNuxtApp().$api as ClienteApi;
}
