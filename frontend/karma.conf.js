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
          statements: 100,
          lines: 100,
          branches: 100,
          functions: 100,
        },
      },
    },

    reporters: ['progress', 'coverage'],

    browsers: ['chromium'],
    singleRun: true,
  });
}
