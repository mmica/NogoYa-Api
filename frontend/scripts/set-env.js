#!/usr/bin/env node
/**
 * Generates src/environments/environment.prod.ts from environment variables
 * BEFORE the Angular build. Vercel exposes vars defined in the project settings
 * (e.g. NG_APP_API_BASE_URL) to the build process.
 *
 * Falling back to localhost keeps `npm run build` working in local sandboxes.
 */
const fs = require('fs');
const path = require('path');

const apiBaseUrl = process.env.NG_APP_API_BASE_URL ?? 'http://localhost:5001/api/v1';
const production = process.env.NODE_ENV === 'production' || process.env.VERCEL === '1';

const target = path.join(__dirname, '..', 'src', 'environments', 'environment.prod.ts');
const contents =
`// AUTO-GENERATED — do not edit by hand. See scripts/set-env.js.
export const environment = {
  production: ${production},
  apiBaseUrl: '${apiBaseUrl}'
};
`;

fs.mkdirSync(path.dirname(target), { recursive: true });
fs.writeFileSync(target, contents);
console.log(`[set-env] Wrote ${target} with apiBaseUrl=${apiBaseUrl}`);
