# User Comparison: Working vs Failing

## Working User: `testuser@example.com`
- **ID**: c679b8c2-2891-414a-f130-08ddd36fc637
- **Email**: testuser@example.com
- **Name**: Test User
- **Country**: United States
- **CitizenshipCountryId**: 34c989df-b46e-4822-9035-77d82215220d (United States)
- **Category**: Immigrate
- **MaritalStatus**: Single
- **State**: CA
- **CreatedAt**: 2025-08-04T16:03:58.8466789

## Failing User: `john@testing.com`
- **ID**: 8089c97a-49fd-48e6-72c1-08ddd38d18c1
- **Email**: John@testing.com
- **Name**: John Smith
- **Country**: Albania
- **CitizenshipCountryId**: a8ffbcb4-b28f-4c1a-836f-6c2a62bb3121 (Albania)
- **Category**: Immigrate
- **MaritalStatus**: Divorced
- **State**: (empty)
- **CreatedAt**: 2025-08-04T19:28:31.3521493

## Key Differences

### 1. **Citizenship Country** (CRITICAL DIFFERENCE)
- **Working**: United States (34c989df-b46e-4822-9035-77d82215220d)
- **Failing**: Albania (a8ffbcb4-b28f-4c1a-836f-6c2a62bb3121)

### 2. **Marital Status**
- **Working**: Single
- **Failing**: Divorced

### 3. **Location Data**
- **Working**: Lives in United States, CA state
- **Failing**: Lives in Albania, no state

## Hypothesis: The Issue is Citizenship-Based Logic

The failing user is from Albania, which likely triggers different visa eligibility logic in the VisaEligibilityService. This could cause:

1. Different visa types to be returned
2. Different filtering logic to be applied
3. Potentially no eligible visas for Albania citizens in certain categories
4. Business logic that fails for non-US citizens

The working user being a US citizen might bypass certain validation checks or follow a different code path.