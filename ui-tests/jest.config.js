module.exports = {
  preset: 'ts-jest',
  testEnvironment: 'node',
  testMatch: ['<rootDir>/ui-tests/**/*.test.ts'],
  setupFilesAfterEnv: ['<rootDir>/ui-tests/setup.ts'],
  testTimeout: 30000,
  collectCoverageFrom: [
    'ui-tests/**/*.ts',
    '!ui-tests/**/*.test.ts',
    '!ui-tests/jest.config.js',
    '!ui-tests/setup.ts'
  ]
};