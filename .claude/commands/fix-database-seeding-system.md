# Fix and Bulletproof Database Seeding System

Create robust database seeding that never fails during testing:

1. **Comprehensive Data Seeder**:
   - Read and seed BaseVisaTypes from update.json
   - Read and seed CategoryClass from CategoryClass.json  
   - Integrate with GovScraper for Countries data
   - Handle foreign key relationships properly

2. **Bulletproof Implementation**:
   - Check for existing data before seeding
   - Use transactions for data consistency
   - Handle duplicate prevention
   - Implement retry logic for failures

3. **Startup Integration**:
   - Call seeding automatically on application start
   - Handle development vs production scenarios
   - Log seeding progress and results
   - Validate seeded data integrity

4. **Migration Safety**:
   - Ensure seeding works after database drops
   - Handle migration rollbacks gracefully
   - Preserve relationships during re-seeding
   - Create backup/restore functionality

Arguments: $ENVIRONMENT $FORCE_RESEED
