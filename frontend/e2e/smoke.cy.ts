describe('PayTrack smoke tests', () => {
  it('redirects anonymous users from the app root to login', () => {
    cy.visit('/');

    cy.location('pathname').should('eq', '/login');
    cy.contains('h1', 'Welcome to PayTrack').should('be.visible');
  });

  it('shows the login screen', () => {
    cy.visit('/login');

    cy.title().should('match', /TUR PayTrack/);
    cy.contains('h1', 'Welcome to PayTrack').should('be.visible');
    cy.get('[data-testid="login-google-button"]').should('be.visible');
  });
});
