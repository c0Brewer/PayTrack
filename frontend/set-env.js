import * as fs from 'fs';
import * as path from 'path';
import { fileURLToPath } from 'url';

import * as dotenv from 'dotenv';

// Fix for __dirname in ESM
const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

// Load .env from repo root
const envRootPath = path.resolve(__dirname, '../.env');
const envExampleRootPath = path.resolve(__dirname, '../env.example');
const envLocalPath = path.resolve(__dirname, '.env');
const envExampleLocalPath = path.resolve(__dirname, 'env.example');

let envPath = '';

if (fs.existsSync(envRootPath)) envPath = envRootPath;
else if (fs.existsSync(envLocalPath)) envPath = envLocalPath;
else if (fs.existsSync(envExampleRootPath)) {
  fs.copyFileSync(envExampleRootPath, envRootPath);
  envPath = envRootPath;
} else if (fs.existsSync(envExampleLocalPath)) {
  fs.copyFileSync(envExampleLocalPath, envLocalPath);
  envPath = envLocalPath;
} else {
  console.error('Could not find or create a .env file');
}

dotenv.config({ path: envPath });

// Decide output file for Angular environment
const targetPath = path.resolve(__dirname, './src/environments/environment.ts');

const environmentFileContent = `export const environment = {
  production: ${process.env.NODE_ENV === 'production'},
  googleClientId: '${process.env.GOOGLE_CLIENT_ID}',
  apiBaseUrl: '${process.env.API_BASE_URL ?? ''}',
};
`;

fs.writeFileSync(targetPath, environmentFileContent, { encoding: 'utf8' });
