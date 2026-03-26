import js from '@eslint/js';
import tseslint from 'typescript-eslint';
import angularParser from "@angular-eslint/template-parser";
import angularTemplate from '@angular-eslint/eslint-plugin-template';
import angular from '@angular-eslint/eslint-plugin';
import importPlugin from 'eslint-plugin-import';
import unusedImports from 'eslint-plugin-unused-imports';
import prettier from 'eslint-plugin-prettier';

export default [
  js.configs.recommended,

  ...tseslint.configs.recommended,

  {
    files: ['**/*.ts'],
    languageOptions: {
      parser: tseslint.parser,
      parserOptions: {
        project: ['./tsconfig.eslint.json'],
      },
    },
    plugins: {
      '@angular-eslint': angular,
      import: importPlugin,
      'unused-imports': unusedImports,
      'prettier': prettier
    },
    rules: {
      'unused-imports/no-unused-imports': 'error',
      '@typescript-eslint/no-unused-vars': 'error',
      '@typescript-eslint/no-explicit-any': 'error',
      '@typescript-eslint/explicit-function-return-type': 'error',

      /* ---------------- Angular ---------------- */

      '@angular-eslint/component-class-suffix': 'error',
      '@angular-eslint/directive-class-suffix': 'error',
      '@angular-eslint/no-empty-lifecycle-method': 'warn',

      /* ---------------- Imports ---------------- */

      'import/order': [
        'error',
        {
          groups: ['builtin', 'external', 'internal', 'parent', 'sibling', 'index'],
          'newlines-between': 'always',
          alphabetize: { order: 'asc', caseInsensitive: true },
        },
      ],

      'no-console': ["error", { "allow": ["error"] }],
      'no-debugger': 'error',
      'prettier/prettier': 'error',
    },
  },
  /* ---------------- Angular HTML templates ---------------- */

  {
    files: ['**/*.html'],
    languageOptions: {
      parser: angularParser,
    },
    plugins: {
      '@angular-eslint/template': angularTemplate,
    },
    rules: {
      '@angular-eslint/template/no-negated-async': 'error',
    },
  },
  {
    files: ['src/main.ts', 'src/server.ts'],
    rules: {
      'no-console': 'off',
    },
  },
  {
    files: ['**/*.js'],
    languageOptions: {
      sourceType: 'script',
    },
  },

  /* ---------------- Ignore generated files ---------------- */

  {
    ignores: [
      "e2e/base.spec.ts",
      ".angular",
      ".vscode",
      "node_modules",
      "dist",
      "karma.conf.js",
      "coverage",
      "src/app/app.ts",
      "src/main.server.ts",
      "src/app/types/api-types.ts"
    ]
  }
];
