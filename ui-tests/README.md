# Law4Hire UI Testing Suite

This directory contains comprehensive UI tests for the Law4Hire application using Puppeteer and Jest.

## Overview

The test suite includes:
- **User Registration Tests**: Complete registration flow testing for all user categories
- **User Workflow Tests**: End-to-end user journey testing
- **Form Validation Tests**: Input validation and error handling
- **Accessibility Tests**: Basic accessibility compliance testing
- **Navigation Tests**: Page navigation and routing tests

## Test Categories

### User Categories Tested
All tests use the standard test password: `SecureTest123!`

1. **Visit** - Tourist/visitor visa applicants
2. **Work** - Employment-based visa applicants  
3. **Study** - Student visa applicants
4. **Family** - Family-based visa applicants
5. **Investment** - Investor visa applicants
6. **Asylum** - Asylum seekers
7. **Immigrate** - General immigration applicants

### Test Files

- `user-registration.test.ts` - Registration form testing
- `user-workflows.test.ts` - Complete user journey testing
- `setup.ts` - Test configuration and helper functions

## Setup

1. Install dependencies:
```bash
npm install
```

2. Ensure the application is running:
- Web app: http://localhost:5161
- API: https://localhost:7280

3. Run tests:
```bash
# Run all UI tests
npm run test:ui

# Run tests in watch mode
npm run test:ui:watch

# Run tests with coverage
npm run test:ui:coverage
```

## Test Configuration

### Environment Variables
- `NODE_ENV=production` - Run tests in headless mode
- `TEST_TIMEOUT` - Override default test timeout (30000ms)

### Configuration (setup.ts)
```typescript
export const TEST_CONFIG = {
  BASE_URL: 'http://localhost:5161',
  API_BASE_URL: 'https://localhost:7280',
  DEFAULT_TIMEOUT: 10000,
  STANDARD_PASSWORD: 'SecureTest123!',
  TEST_EMAIL_DOMAIN: '@testing.com'
};
```

## Helper Functions

### Test User Generation
```typescript
const testUser = generateTestUser('Work');
// Creates: { firstName: 'TestWork', lastName: 'User', email: 'work123@testing.com', ... }
```

### Form Interaction
```typescript
await fillFormField(page, 'input[name="email"]', testUser.email);
await selectDropdownOption(page, 'select[name="category"]', 'Work');
await clickButton(page, 'button[type="submit"]');
```

### Navigation and Waiting
```typescript
await waitForPageLoad(page);
await waitForText(page, 'Registration successful');
```

## Test Strategy

### Registration Testing
- Tests all user categories with standard password
- Validates form submission and error handling
- Checks duplicate email prevention
- Verifies password strength requirements

### Workflow Testing  
- Complete user journeys from registration to dashboard
- Category-specific flows (visa interviews, pricing, etc.)
- Navigation between main application pages
- Login/logout functionality

### Error Handling
- Network error simulation
- Invalid input handling
- Protected route access
- Graceful degradation testing

### Accessibility
- Form label associations
- Keyboard navigation support
- Screen reader compatibility basics

## Debugging Tests

### Running Individual Tests
```bash
npx jest ui-tests/user-registration.test.ts --config=ui-tests/jest.config.js
```

### Headful Mode (Visual Debugging)
Set `NODE_ENV=development` to run tests with visible browser:
```bash
NODE_ENV=development npm run test:ui
```

### Screenshots on Failure
Tests automatically capture screenshots on failure (when configured).

## Maintenance

### Adding New Tests
1. Create test file in `ui-tests/` directory
2. Import helpers from `setup.ts`
3. Follow existing naming conventions
4. Use standard test password for consistency

### Updating Selectors
When UI changes, update selectors in test files:
- Use data attributes when possible: `[data-testid="submit-button"]`
- Provide fallback selectors: `'button[type="submit"], .submit-btn'`
- Use semantic selectors: `'input[name="email"], #email'`

### Test Data Management
- All test users use `@testing.com` domain
- Unique identifiers prevent test conflicts
- Standard password enables manual testing

## Integration with CI/CD

### Prerequisites
- Application must be running on expected ports
- Database must be seeded with test data
- SSL certificates must be trusted for API calls

### Test Execution Order
1. Unit tests (fastest)
2. Integration tests (medium)
3. UI tests (slowest)

### Parallel Execution
Tests are designed to run independently and can be parallelized:
```bash
npx jest --maxWorkers=4 ui-tests/
```

## Troubleshooting

### Common Issues

1. **Tests timeout**
   - Increase timeout in jest.config.js
   - Check application is running on correct ports
   - Verify network connectivity

2. **Element not found**
   - Check if UI has changed
   - Update selectors in test files
   - Verify page load timing

3. **Certificate errors**
   - API uses HTTPS with self-signed certificates
   - Tests configured to ignore certificate errors
   - Ensure `--ignore-certificate-errors` flag is set

4. **Database conflicts**
   - Tests create unique users with timestamps
   - Database should be reset between test runs
   - Check for existing test data

### Debug Mode
Enable verbose logging:
```bash
DEBUG=puppeteer:* npm run test:ui
```

## Test Coverage

The test suite covers:
- ✅ User registration (all categories)
- ✅ Form validation
- ✅ Error handling
- ✅ Basic navigation
- ✅ Login/logout flows
- ✅ Password requirements
- ✅ Email validation
- ✅ Responsive design basics
- ✅ Accessibility basics

### Future Enhancements
- [ ] Payment processing tests
- [ ] Document upload tests
- [ ] Advanced visa interview flows
- [ ] Email notification testing
- [ ] Performance testing
- [ ] Cross-browser testing
- [ ] Visual regression testing

## Contributing

1. Follow existing test patterns
2. Use descriptive test names
3. Add comments for complex test logic
4. Update this README when adding new test categories
5. Ensure tests are deterministic and can run independently