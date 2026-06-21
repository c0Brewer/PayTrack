import { defineConfig } from 'cypress';

export default defineConfig({
  allowCypressEnv: false,
  video: false,
  screenshotsFolder: 'cypress/screenshots',
  videosFolder: 'cypress/videos',
  downloadsFolder: 'cypress/downloads',
  e2e: {
    baseUrl: 'http://127.0.0.1:4200',
    fixturesFolder: 'e2e/fixtures',
    specPattern: 'e2e/**/*.cy.{js,jsx,ts,tsx}',
    supportFile: false,
  },
});
