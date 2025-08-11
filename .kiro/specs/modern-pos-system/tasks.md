# Implementation Plan

- [x] 1. Project Setup and Core Infrastructure
  - [x] Create .NET 8 solution structure with modular monolith architecture
  - [x] Set up Entity Framework Core with PostgreSQL
  - [x] Configure dependency injection container for modules
  - [x] Implement shared kernel with common domain objects and interfaces
  - [x] Set up Hangfire for background job processing
  - [x] Configure JWT authentication and authorization policies
  - _Requirements: All modules foundation_

- [x] 2. Database Foundation and Multi-Tenancy (Database-per-Tenant)
  - [x] 2.1 Control-Plane Database
    - [x] Create `ControlPlaneDbContext` with table `tenant_connection_strings`
    - [x] Add EF migration for control-plane DB and apply it
    - [ ] Optional: add minimal `organizations` registry in control-plane
    - _Requirements: 2.4_

  - [x] 2.2 Tenant Context & Resolution
    - [x] Implement `ITenantMetadataStore` to read tenant connection strings
    - [x] Implement `ITenantConnectionResolver` to resolve per-tenant connection strings
    - [x] Wire resolver and context in DI; ensure `POSDbContext` uses tenant-specific connection
    - _Requirements: 2.4_

  - [x] 2.3 Tenant Database Schema
    - [x] Update `POSDbContext` to remove schema switching; use unqualified table mappings
    - [ ] Create/update EF migrations for tenant DB tables (organizations, branches, users, roles, etc.)
    - [x] Ensure soft-delete filters and audit fields are applied
    - _Requirements: 2.1, 2.4_

  - [x] 2.4 Tenant Provisioning & Seeding

    - [x] Implement provisioning service to create tenant DB and apply migrations
    - [x] Seed tenant defaults (admin user, roles, settings)
    - [x] Store encrypted tenant connection string in control-plane (read path supports protection)
    - [x] Add health check endpoint to validate tenant connectivity
    - _Requirements: 2.1, 2.4_

  - [ ] 2.5 Operational Automation
    - [x] Background job to migrate out-of-date tenant databases
    - [ ] Scripts/runbooks for backup/restore per tenant
    - _Requirements: 2.4_

- [ ] 3. Authentication and Authorization Module
  - [x] 3.1 Implement JWT authentication service
    - [x] Create User entity and authentication models
    - [x] Implement JWT token generation and validation
    - [x] Add refresh token functionality
    - [ ] Write unit tests for authentication logic
    - _Requirements: 1.1, 1.2_

  - [x] 3.2 Implement role-based authorization
    - [x] Create Role and Permission entities
    - [x] Implement authorization policies and handlers
    - [x] Add role assignment and permission checking
    - [ ] Write unit tests for authorization logic
    - _Requirements: 1.2, 1.3_

  - [x] 3.3 Add user management functionality
    - [x] Implement user CRUD operations (Create + List + Update + Delete)
    - [x] Add staff assignment to branches (assign/unassign/set default)
    - [ ] Implement session logging and activity tracking
    - [ ] Write integration tests for user management
    - _Requirements: 1.4, 1.5_

- [x] 4. Branch Management Module
  - [x] 4.1 Implement branch entity and services
    - [x] Create Branch entity with settings support
    - [x] Implement branch CRUD operations
    - [ ] Add branch-specific configuration management
    - [ ] Write unit tests for branch operations
    - _Requirements: 2.1, 2.2_

  - [x] 4.2 Add multi-branch data isolation
    - [x] Implement tenant-aware repository pattern
    - [x] Add branch-level data filtering (via org-scoped context)
    - [x] Create branch assignment for users
    - [ ] Create device assignment endpoints
    - [ ] Write integration tests for data isolation
    - _Requirements: 2.3, 2.4_

- [x] 5. Product and Inventory Module
  - [x] 5.1 Implement product management
    - [x] Create Product entity
    - [x] Implement product CRUD operations (API + service)
    - [x] Add barcode and SKU management (unique indexes)
    - [x] Add categories and attributes (basic fields)
    - [x] Image support (URL field)
    - [ ] Write unit tests for product operations
    - _Requirements: 3.1, 3.5_

  - [x] 5.2 Implement inventory tracking system
    - [x] Create InventoryItem entity for multi-branch stock
    - [x] Implement inventory read and stock adjustment (API + service)
    - [x] Implement stock movement tracking and audit trail (basic)
    - [x] Add reorder level monitoring and alerts (alerts entity + endpoints + job)
    - [ ] Write integration tests for inventory operations
    - _Requirements: 3.2, 3.3, 3.4_

  - [x] 5.3 Add stock adjustment functionality
    - [x] Implement stock adjustment operations (request + approve + apply)
    - [x] Add stocktaking support with variance reporting (sessions, lines, finalize with optional adjustments)
    - [x] Create approval workflow for adjustments
    - [ ] Write unit tests for stock adjustments
    - _Requirements: 6.1, 6.3, 6.4_

- [x] 6. Sales and Checkout Module
  - [x] 6.1 Implement cart management system
    - [x] Create Cart and CartItem entities
    - [x] Implement cart operations (create, add, remove)
    - [x] Add cart persistence
    - [ ] Write unit tests for cart functionality
    - _Requirements: 7.2_

  - [x] 6.2 Implement sales transaction processing
    - [x] Create Sale and SaleItem entities
    - [x] Implement transaction creation and processing (checkout)
    - [x] Add inventory deduction on sales
    - [ ] Write integration tests for sales processing
    - _Requirements: 7.1, 7.2_

  - [x] 6.3 Add payment processing
    - [x] Implement multiple payment method support
    - [x] Add split payment functionality
    - [x] Create payment validation and recording
    - [ ] Write unit tests for payment processing
    - _Requirements: 7.3_

  - [x] 6.4 Implement refunds and returns
    - [x] Add refund processing functionality
    - [x] Implement return handling with inventory adjustment
    - [x] Create refund approval workflow (Pending/Approved/Rejected + policy)
    - [ ] Write integration tests for refunds and returns
    - _Requirements: 7.5_

- [x] 7. Customer Management Module
  - [x] 7.1 Implement customer entity and services
    - [x] Create Customer entity with contact information
    - [x] Implement customer CRUD operations
    - [x] Add customer search and filtering
    - [ ] Write unit tests for customer operations
    - _Requirements: 9.1, 9.4_

  - [x] 7.2 Add customer purchase history tracking
    - [x] Implement purchase history association (sales linked to customer)
    - [x] Create customer analytics and insights (total spent, avg order, recency)
    - [x] Add customer loyalty tracking foundation (points + tier entity and APIs)
    - [ ] Write integration tests for purchase history
    - _Requirements: 9.2, 9.3_

- [x] 8. Web API and Controllers
  - [x] 8.1 Create authentication API endpoints
    - [x] Implement login, logout, and token refresh endpoints
    - [x] Add user management API controllers
    - [ ] Implement proper error handling and validation
    - [ ] Write API integration tests
    - _Requirements: 1.1, 1.5_

  - [x] 8.2 Create product and inventory API endpoints
    - [x] Implement product management API controllers
    - [x] Add inventory tracking API endpoints
    - [ ] Create barcode scanning support endpoints
    - [ ] Write API integration tests
    - _Requirements: 3.1, 3.2, 7.1_

  - [x] 8.3 Create sales and checkout API endpoints
    - [x] Implement cart management API controllers
    - [x] Add sales processing API endpoints
    - [x] Create payment processing endpoints
    - [ ] Write API integration tests
    - _Requirements: 7.2, 7.3_

- [ ] 9. Blazor Frontend Foundation
  - [ ] 9.1 Set up Blazor application structure
    - Create Blazor Server/WASM hybrid setup
    - Implement authentication components
    - Add navigation and layout components
    - Configure Tailwind CSS styling
    - _Requirements: 14.1_

  - [ ] 9.2 Implement login and user management UI
    - Create login page with form validation
    - Implement user management interface
    - Add role assignment UI components
    - Write UI component tests
    - _Requirements: 1.1, 1.2, 1.5_

  - [ ] 9.3 Create product management interface
    - Implement product listing and search UI
    - Create product add/edit forms with image upload
    - Add barcode scanning interface
    - Write UI component tests
    - _Requirements: 3.1, 3.5_

- [ ] 10. Point of Sale Interface
  - [ ] 10.1 Implement POS checkout interface
    - Create product search and selection UI
    - Implement shopping cart interface
    - Add barcode scanning functionality
    - Write UI component tests
    - _Requirements: 7.1, 7.2_

  - [ ] 10.2 Add payment processing UI
    - Create payment method selection interface
    - Implement split payment UI
    - Add receipt generation and printing
    - Write integration tests for checkout flow
    - _Requirements: 7.3, 8.1, 8.3_

- [ ] 11. Real-time Updates with SignalR
  - [ ] 11.1 Implement SignalR hubs
    - Create inventory update hub for real-time stock changes
    - Implement sales notification hub
    - Add user activity tracking hub
    - Write SignalR integration tests
    - _Requirements: 3.2, 7.2, 12.2_

  - [ ] 11.2 Add real-time UI updates
    - Connect Blazor components to SignalR hubs
    - Implement real-time inventory updates in UI
    - Add live sales notifications
    - Write end-to-end tests for real-time features
    - _Requirements: 3.3, 7.2_

- [ ] 12. Offline Capability Foundation
  - [ ] 12.1 Implement offline data storage
    - Create SQLite database for offline operations
    - Implement data synchronization models
    - Add offline transaction queuing
    - Write unit tests for offline storage
    - _Requirements: 7.4, 14.3_

  - [ ] 12.2 Add synchronization service
    - Implement background sync service
    - Create conflict resolution strategies
    - Add sync status tracking and UI
    - Write integration tests for synchronization
    - _Requirements: 7.4, 14.3_

- [ ] 13. Reporting Module Foundation
  - [ ] 13.1 Implement basic reporting infrastructure
    - Create report generation service
    - Implement sales report queries
    - Add inventory report functionality
    - Write unit tests for report generation
    - _Requirements: 10.1, 10.2_

  - [ ] 13.2 Create reporting dashboard
    - Implement dashboard UI with key metrics
    - Add chart and graph components
    - Create report export functionality
    - Write UI tests for dashboard
    - _Requirements: 10.3, 10.4_

- [x] 14. Advanced Inventory Features
  - [x] 14.1 Implement supplier management
    - [x] Create Supplier entity and services (CRUD + search API)
    - [x] Implement purchase order functionality
    - [x] Add supplier-specific pricing
    - [ ] Write unit tests for supplier operations
    - _Requirements: 4.1, 4.2, 4.4_

  - [x] 14.2 Add purchase order processing
    - [x] Define PO/GRN entities and mappings
    - [x] Implement PO creation and approval workflow
    - [x] Create goods received note (GRN) functionality (updates inventory and average cost)
    - [x] Add purchase returns processing
    - [ ] Write integration tests for purchase workflow
    - _Requirements: 4.2, 4.3_

- [ ] 15. Cost Tracking and Profitability
  - [x] 15.1 Implement cost tracking system
    - [x] Add average cost calculation on purchases (GRN)
    - [x] Implement COGS tracking per sale (item-level and sale-level)
    - [x] Create gross profit calculation (sale-level)
    - [ ] Write unit tests for cost calculations
    - _Requirements: 5.1, 5.2, 5.3_

  - [x] 15.2 Add profitability reporting
    - [x] Create profit margin reports (summary)
    - [x] Implement product profitability analysis
    - [ ] Add cost trend reporting
    - [ ] Write integration tests for profitability features
    - _Requirements: 5.4_

- [ ] 16. Mobile App Foundation
  - [ ] 16.1 Create .NET MAUI mobile app
    - Set up MAUI project structure
    - Implement authentication in mobile app
    - Create basic navigation and UI
    - Write mobile app unit tests
    - _Requirements: 14.1, 14.2_

  - [ ] 16.2 Add mobile POS functionality
    - Implement mobile checkout interface
    - Add camera-based barcode scanning
    - Create offline transaction support
    - Write mobile integration tests
    - _Requirements: 14.2, 14.3_

- [ ] 17. Progressive Web App Features
  - [ ] 17.1 Implement PWA capabilities
    - Add service worker for offline functionality
    - Create app manifest for installation
    - Implement push notifications
    - Write PWA feature tests
    - _Requirements: 14.1, 14.4_

  - [ ] 17.2 Add PWA-specific features
    - Implement background sync for PWA
    - Add offline indicator and sync status
    - Create PWA-optimized UI components
    - Write end-to-end PWA tests
    - _Requirements: 14.3, 14.4_

- [ ] 18. Advanced Customer Features
  - [x] 18.1 Implement loyalty program foundation
    - [x] Create loyalty program entities
    - [x] Implement point accumulation system
    - [x] Add customer tier management
    - [ ] Write unit tests for loyalty features
    - _Requirements: 9.2, 9.3_

  - [x] 18.2 Add customer credit management
    - [x] Implement credit limit tracking
    - [x] Create outstanding balance management
    - [x] Add payment tracking for credit customers
    - [ ] Write integration tests for credit management
    - _Requirements: 9.4_

- [ ] 19. Recipe and BOM Module
  - [ ] 19.1 Implement recipe management
    - Create Recipe and RecipeIngredient entities
    - Implement recipe CRUD operations
    - Add ingredient cost calculation
    - Write unit tests for recipe functionality
    - _Requirements: 15.1, 15.4_

  - [ ] 19.2 Add automatic inventory deduction
    - Implement ingredient deduction on recipe item sales
    - Create batch production support
    - Add recipe cost tracking
    - Write integration tests for recipe inventory integration
    - _Requirements: 15.2, 15.3_

- [ ] 20. Promotion and Discount Engine
  - [x] 20.1 Implement discount system
    - [x] Create Promotion and Discount entities
    - [x] Implement discount calculation engine
    - [x] Add promo code functionality
    - [ ] Write unit tests for discount calculations
    - _Requirements: 16.1, 16.3_

  - [x] 20.2 Add advanced promotion features
    - [x] Implement scheduled promotions
    - [x] Create BOGO and combo deal support
    - [x] Add loyalty tier-based discounts
    - [ ] Write integration tests for promotion engine
    - _Requirements: 16.1, 16.2, 16.4_

- [ ] 21. Background Jobs and Automation
  - [ ] 21.1 Implement background job processing
    - [x] Set up Hangfire for background jobs
    - [x] Create automated stock alert jobs (job + endpoint to trigger per tenant)
    - [ ] Implement data cleanup and maintenance jobs
    - [ ] Write unit tests for background jobs
    - _Requirements: 3.3, 12.3_

  - [ ] 21.2 Add automated reporting
    - [ ] Create scheduled report generation
    - [ ] Implement automated email reports
    - [ ] Add performance monitoring jobs
    - [ ] Write integration tests for automated features
    - _Requirements: 10.3, 12.3_

- [ ] 22. Security Hardening
  - [ ] 22.1 Implement security best practices
    - Add input validation and sanitization
    - Implement rate limiting
    - Create audit logging system
    - Write security tests
    - _Requirements: 1.4, 12.2_

  - [ ] 22.2 Add data protection features
    - Implement data encryption for sensitive fields
    - Create GDPR compliance features
    - Add secure file upload handling
    - Write security integration tests
    - _Requirements: 1.4, 8.4_

- [ ] 23. Performance Optimization
  - [ ] 23.1 Implement caching strategies
    - Add Redis caching for frequently accessed data
    - Implement query optimization
    - Create database indexing strategy
    - Write performance tests
    - _Requirements: All modules performance_

  - [ ] 23.2 Add monitoring and diagnostics
    - Implement Application Insights integration
    - Create custom metrics and dashboards
    - Add health check endpoints
    - Write monitoring integration tests
    - _Requirements: All modules monitoring_

- [ ] 24. Advanced Features Integration
  - [ ] 24.1 Implement table management (restaurant specific)
    - Create Table and Reservation entities
    - Implement table status tracking
    - Add reservation management
    - Write unit tests for table management
    - _Requirements: 17.1, 17.2, 17.3_

  - [ ] 24.2 Add delivery management
    - Create delivery tracking system
    - Implement delivery staff assignment
    - Add delivery zone management
    - Write integration tests for delivery features
    - _Requirements: 18.1, 18.2, 18.3_

- [ ] 25. Testing Implementation
  - [ ] 25.1 Unit testing implementation
    - [ ] Write unit tests for authentication logic
    - [ ] Write unit tests for authorization logic
    - [ ] Write unit tests for product operations
    - [ ] Write unit tests for cart functionality
    - [ ] Write unit tests for payment processing
    - [ ] Write unit tests for customer operations
    - [ ] Write unit tests for supplier operations
    - [ ] Write unit tests for cost calculations
    - [ ] Write unit tests for loyalty features
    - [ ] Write unit tests for discount calculations
    - [ ] Write unit tests for background jobs
    - _Requirements: All modules testing_

  - [ ] 25.2 Integration testing implementation
    - [ ] Write integration tests for user management
    - [ ] Write integration tests for data isolation
    - [ ] Write integration tests for inventory operations
    - [ ] Write integration tests for sales processing
    - [ ] Write integration tests for refunds and returns
    - [ ] Write integration tests for purchase history
    - [ ] Write integration tests for purchase workflow
    - [ ] Write integration tests for credit management
    - [ ] Write integration tests for promotion engine
    - [ ] Write integration tests for automated features
    - _Requirements: All modules integration testing_

  - [ ] 25.3 API testing implementation
    - [ ] Write API integration tests for authentication endpoints
    - [ ] Write API integration tests for product and inventory endpoints
    - [ ] Write API integration tests for sales and checkout endpoints
    - _Requirements: API testing coverage_

- [ ] 26. Database Migrations and Schema Updates
  - [ ] 26.1 Complete tenant database migrations
    - [ ] Create/update EF migrations for tenant DB tables (organizations, branches, users, roles, etc.)
    - [ ] Ensure all domain entities have proper database configurations
    - [ ] Add database indexes for performance optimization
    - [ ] Validate migration scripts work across different environments
    - _Requirements: 2.1, 2.4_

- [ ] 27. Error Handling and Validation
  - [ ] 27.1 Implement comprehensive error handling
    - [ ] Implement proper error handling and validation for authentication API
    - [ ] Add input validation for all API endpoints
    - [ ] Create global exception handling middleware
    - [ ] Implement user-friendly error messages
    - _Requirements: All modules error handling_

- [ ] 28. Configuration and Settings Management
  - [ ] 28.1 Branch-specific configuration
    - [ ] Add branch-specific configuration management
    - [ ] Implement device assignment endpoints
    - [ ] Create barcode scanning support endpoints
    - _Requirements: 2.1, 2.2, 7.1_

- [ ] 29. Multi-Currency Support Implementation


  - [x] 29.1 Currency management foundation



    - [ ] Create Currency and ExchangeRate domain entities
    - [ ] Implement currency CRUD operations and services
    - [ ] Add currency configuration endpoints (set base currency, add/remove currencies)
    - [ ] Create exchange rate management with manual entry support
    - [ ] Write unit tests for currency operations
    - _Requirements: 13.1, 13.2_

  - [ ] 29.2 Exchange rate management
    - [ ] Implement exchange rate history tracking
    - [ ] Add automatic exchange rate updates from external providers (optional)
    - [ ] Create exchange rate conversion utilities
    - [ ] Add exchange rate validation and business rules
    - [ ] Write unit tests for exchange rate calculations
    - _Requirements: 13.2, 13.6_

  - [ ] 29.3 Multi-currency sales processing
    - [ ] Update Sale and SaleItem entities to support multiple currencies
    - [ ] Modify sales processing to handle currency selection and conversion
    - [ ] Update payment processing to support different payment currencies
    - [ ] Implement currency-specific pricing for products
    - [ ] Write integration tests for multi-currency sales
    - _Requirements: 13.3, 13.4, 13.7_

  - [ ] 29.4 Multi-currency reporting and display
    - [ ] Update reporting to show amounts in base currency with original currency details
    - [ ] Implement currency formatting and display utilities
    - [ ] Add currency selection in UI components
    - [ ] Update receipt generation to show proper currency symbols and formatting
    - [ ] Write integration tests for multi-currency reporting
    - _Requirements: 13.5, 13.8_

  - [ ] 29.5 Refund and adjustment handling
    - [ ] Update refund processing to use original transaction exchange rates
    - [ ] Modify stock adjustments to handle currency implications
    - [ ] Add currency consistency validation for financial operations
    - [ ] Write unit tests for multi-currency refunds and adjustments
    - _Requirements: 13.6_

- [ ] 30. Final Integration and Testing
  - [ ] 30.1 End-to-end testing suite
    - [ ] Create comprehensive E2E test scenarios
    - [ ] Implement automated UI testing
    - [ ] Add performance and load testing
    - [ ] Write deployment verification tests
    - _Requirements: All requirements validation_

  - [ ] 30.2 Production readiness
    - [ ] Implement deployment scripts and CI/CD
    - [ ] Create production configuration
    - [ ] Add monitoring and alerting setup
    - [ ] Write deployment documentation
    - _Requirements: System deployment and operations_