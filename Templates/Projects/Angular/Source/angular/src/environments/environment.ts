import { Environment } from '@abp/ng.core';

const baseUrl = 'http://localhost:4200';

export const environment = {
  production: false,
  application: {
    baseUrl,
    name: 'MyProduct',
    logoUrl: '',
  },
  oAuthConfig: {
    issuer: 'https://localhost:44373',
    redirectUri: baseUrl,
    clientId: 'MyProduct_App',
    responseType: 'code',
    scope: 'offline_access openid profile role email phone MyProduct',
    requireHttps: true
  },
  apis: {
    default: {
      url: 'https://localhost:44373',
      rootNamespace: 'MyCompany.MyProduct',
    },
  },
} as Environment;
