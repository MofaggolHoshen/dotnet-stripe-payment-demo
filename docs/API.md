# API Documentation

## Overview

Complete REST API documentation for the Stripe Payment Demo project.

## Endpoints

### Customer Endpoints

#### Register Customer

- **POST** `/api/customers`
- Create a new customer in Stripe

#### Get Customer

- **GET** `/api/customers/{customerId}`
- Retrieve customer details

---

### Subscription Endpoints

#### Create Subscription

- **POST** `/api/subscriptions`
- Create a new subscription for a customer

#### Get Subscription

- **GET** `/api/subscriptions/{subscriptionId}`
- Retrieve subscription details

#### Update Subscription

- **PUT** `/api/subscriptions/{subscriptionId}`
- Update subscription (change plan, payment method)

#### Cancel Subscription

- **DELETE** `/api/subscriptions/{subscriptionId}`
- Cancel subscription

#### List Subscriptions

- **GET** `/api/subscriptions`
- List all subscriptions for a customer

---

### Invoice Endpoints

#### Get Invoice

- **GET** `/api/invoices/{invoiceId}`
- Retrieve invoice details

#### List Invoices

- **GET** `/api/invoices`
- List invoices for a customer

#### Download Invoice PDF

- **GET** `/api/invoices/{invoiceId}/pdf`
- Download invoice as PDF

---

### Webhook Endpoint

#### Receive Webhook

- **POST** `/api/webhooks/stripe`
- Receive Stripe webhook events (signature validation required)

---

**More details coming in Phase 7**
