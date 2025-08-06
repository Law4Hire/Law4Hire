# Implement SQL Server BLOB File Storage

Create document management system using SQL Server for file storage:

1. **Database Schema**:
   - Create UserDocuments table with BLOB storage
   - Implement file metadata tracking
   - Set up proper indexing for performance
   - Handle file versioning and history

2. **File Management Service**:
   - Upload files to SQL Server VARBINARY column
   - Implement secure file retrieval
   - Add file type validation and limits
   - Create file compression for large documents

3. **API Endpoints**:
   - File upload with progress tracking
   - Secure file download with permissions
   - File listing and metadata retrieval
   - File deletion and cleanup

4. **Security and Performance**:
   - User-based access control
   - File virus scanning integration
   - Efficient binary data handling
   - Thumbnail generation for images

Arguments: $FILE_TYPE $MAX_SIZE_MB
