# Spec Delta

## Purpose

Creation of Orders with credit limit validation and per-item discount calculation, covering the Domain, Application, Infrastructure and API layers.

## ADDED Requirements

### Requirement: Order creation
The system SHALL allow a customer to create an Order containing one or more OrderItems.

#### Scenario: Valid order is created
- **WHEN** a customer creates an Order with at least one OrderItem
- **THEN** the Order is persisted and receives an identifier

#### Scenario: Order without items is rejected
- **WHEN** a customer attempts to create an Order without items
- **THEN** the creation is rejected

### Requirement: Order input validation
The system SHALL reject Order creation when required data is invalid.

#### Scenario: Order without a customer is rejected
- **WHEN** an Order is created without a customer
- **THEN** the creation is rejected

#### Scenario: OrderItem with zero quantity is rejected
- **WHEN** an Order is created and any OrderItem has zero quantity
- **THEN** the creation is rejected

#### Scenario: OrderItem with non-positive unit price is rejected
- **WHEN** an Order is created and any OrderItem has a non-positive unit price
- **THEN** the creation is rejected

### Requirement: Credit limit validation
The system SHALL reject the creation of an Order when its total exceeds the customer's credit limit.

#### Scenario: Total within the credit limit
- **WHEN** the order total is less than or equal to the customer's credit limit
- **THEN** the Order is created

#### Scenario: Total exceeds the credit limit
- **WHEN** the order total is greater than the customer's credit limit
- **THEN** the Order creation is rejected with a credit limit exceeded reason and the Order is not persisted

### Requirement: Per-item discount calculation
The system SHALL calculate a discount per item and reduce each OrderItem's line total accordingly.

#### Scenario: Item line total is reduced by its discount
- **WHEN** an Order is created
- **THEN** each OrderItem's line total equals its unit price times quantity minus the item's discount, and the order total is the sum of the discounted line totals

#### Scenario: Item without discount
- **WHEN** an Order is created and an OrderItem has no discount configured
- **THEN** that item's line total equals its unit price times quantity
