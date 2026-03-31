export default function(config) {
  config.set({
    frameworks: ['jasmine', '@angular-devkit/build-angular'],
    plugins: [
      require('karma-chrome-launcher'),
      require('@angular-devkit/build-angular/plugins/karma'),
      require('karma-coverage'),
    ],

    coverageReporter: {
      dir: join(__dirname, 'coverage'),
      subdir: '.',
      fixWebpackSourcePaths: true,
      reporters: [
        { type: 'text-summary', file: 'coverage.txt' },
        { type: 'html' },
        { type: 'lcovonly', subdir: './' },
      ],
      check: {
        global: {
          statements: 80,
          lines: 80,
          branches: 80,
          functions: 80,
        },
      },
    },

    reporters: ['progress', 'coverage'],

    browsers: ['chromium'],
    singleRun: true,
  });
}
