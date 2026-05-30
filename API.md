# Stripe Payment Demo - API Documentation

## Base URL

```
https://localhost:7001/api
```

## Authentication

Currently, the API does not require authentication. Authentication will be implemented in Phase 9.

## Response Format

All API responses follow this format:

### Success Response

```json
{
  "message": "Operation successful",
  "data": {},
  "error": null
}
```

### Error Response

```json
{
  "error": "Error message describing the failure"
}
```

## HTTP Status Codes

- **200 OK** - Request succeeded
- **201 Created** - Resource created
- **400 Bad Request** - Invalid request parameters
- **404 Not Found** - Resource not found
- **500 Internal Server Error** - Server error

---

## Customer Endpoints

### Register Customer

Creates a new customer in Stripe and stores it locally.

**Endpoint:** `POST /api/customer/register`

**Request Body:**

```json
{
  "email": "customer@example.com",
  "name": "John Doe"
}
```

**Response (201 Created):**

```json
{
  "message": "Customer registered successfully",
  "customerId": 1,
  "stripeCustomerId": "cus_1234567890"
}
```

**Error Response (400):**

```json
{
  "error": "Email is required"
}
```

---

### Get Customer

Retrieves customer details.

**Endpoint:** `GET /api/customer/{customerId}`

**URL Parameters:**

- `customerId` (int) - Local customer ID

**Response (200 OK):**

```json
{
  "id": 1,
  "stripeCustomerId": "cus_1234567890",
  "email": "customer@example.com",
  "name": "John Doe",
  "defaultPaymentMethodId": null,
  "billingAddress": null,
  "stripeCreatedAt": "2026-05-30T10:00:00Z",
  "createdAt": "2026-05-30T10:00:00Z",
  "updatedAt": "2026-05-30T10:00:00Z"
}
```

**Error Response (404):**

```json
{
  "error": "Customer not found"
}
```

---

### List Customers

Lists all customers with pagination.

**Endpoint:** `GET /api/customer`

**Query Parameters:**

- `limit` (int, optional) - Results per page (default: 10)
- `page` (int, optional) - Page number (default: 1)

**Response (200 OK):**

```json
{
  "data": [
    {
      "id": 1,
      "stripeCustomerId": "cus_1234567890",
      "email": "customer@example.com",
      "name": "John Doe",
      "createdAt": "2026-05-30T10:00:00Z"
    }
  ],
  "pagination": {
    "page": 1,
    "limit": 10,
    "total": 1,
    "pages": 1
  }
}
```

---

### Update Customer

Updates customer information.

**Endpoint:** `PUT /api/customer/{customerId}`

**URL Parameters:**

- `customerId` (int) - Local customer ID

**Request Body:**

```json
{
  "email": "newemail@example.com",
  "name": "Jane Doe"
}
```

**Response (200 OK):**

```json
{
  "message": "Customer updated successfully"
}
```

---

### Delete Customer

Deletes a customer from Stripe and locally.

**Endpoint:** `DELETE /api/customer/{customerId}`

**URL Parameters:**

- `customerId` (int) - Local customer ID

**Response (200 OK):**

```json
{
  "message": "Customer deleted successfully"
}
```

---

## Subscription Endpoints

### Create Subscription

Creates a new subscription for a customer.

**Endpoint:** `POST /api/subscription/create`

**Request Body:**

```json
{
  "customerId": 1,
  "stripePriceId": "price_1234567890",
  "metadata": {
    "plan_name": "premium",
    "trial_days": "14"
  }
}
```

**Response (200 OK):**

```json
{
  "message": "Subscription created successfully",
  "subscriptionId": 1,
  "stripeSubscriptionId": "sub_1234567890",
  "status": "active"
}
```

**Parameters:**

- `customerId` (int, required) - Local customer ID
- `stripePriceId` (string, required) - Stripe price ID (e.g., `price_...`)
- `metadata` (object, optional) - Additional metadata

---

### Get Subscription

Retrieves subscription details.

**Endpoint:** `GET /api/subscription/{subscriptionId}`

**URL Parameters:**

- `subscriptionId` (int) - Local subscription ID

**Response (200 OK):**

```json
{
  "id": 1,
  "stripeSubscriptionId": "sub_1234567890",
  "customerId": 1,
  "stripePriceId": "price_1234567890",
  "stripeProductId": "prod_1234567890",
  "status": "active",
  "currentPeriodStart": "2026-05-30T10:00:00Z",
  "currentPeriodEnd": "2026-06-30T10:00:00Z",
  "endedAt": null,
  "canceledAt": null,
  "amount": 9999,
  "billingInterval": "month",
  "billingIntervalCount": 1,
  "stripeCreatedAt": "2026-05-30T10:00:00Z",
  "createdAt": "2026-05-30T10:00:00Z",
  "updatedAt": "2026-05-30T10:00:00Z"
}
```

**Statuses:**

- `active` - Subscription is active
- `past_due` - Payment failed
- `canceled` - Subscription has been canceled
- `unpaid` - Subscription payment is overdue
- `incomplete` - Initial payment incomplete

---

### List Customer Subscriptions

Lists all subscriptions for a customer.

**Endpoint:** `GET /api/subscription/customer/{customerId}`

**URL Parameters:**

- `customerId` (int) - Local customer ID

**Query Parameters:**

- `status` (string, optional) - Filter by status (active, canceled, etc.)

**Response (200 OK):**

```json
{
  "data": [
    {
      "id": 1,
      "stripeSubscriptionId": "sub_1234567890",
      "status": "active",
      "currentPeriodEnd": "2026-06-30T10:00:00Z"
    }
  ],
  "count": 1
}
```

---

### Update Subscription

Updates subscription (e.g., change plan).

**Endpoint:** `PUT /api/subscription/{subscriptionId}`

**URL Parameters:**

- `subscriptionId` (int) - Local subscription ID

**Request Body:**

```json
{
  "newStripePriceId": "price_9876543210",
  "metadata": {
    "updated_reason": "upgrade"
  }
}
```

**Response (200 OK):**

```json
{
  "message": "Subscription updated successfully",
  "subscription": {
    "id": 1,
    "stripeSubscriptionId": "sub_1234567890",
    "status": "active"
  }
}
```

---

### Cancel Subscription

Cancels a subscription.

**Endpoint:** `POST /api/subscription/{subscriptionId}/cancel`

**URL Parameters:**

- `subscriptionId` (int) - Local subscription ID

**Request Body (optional):**

```json
{
  "immediately": false
}
```

**Parameters:**

- `immediately` (boolean, optional) - Cancel immediately (true) or at period end (false, default)

**Response (200 OK):**

```json
{
  "message": "Subscription canceled successfully",
  "status": "canceled",
  "canceledAt": "2026-05-30T10:30:00Z"
}
```

---

### Sync Subscription

Syncs subscription status from Stripe.

**Endpoint:** `POST /api/subscription/{subscriptionId}/sync`

**URL Parameters:**

- `subscriptionId` (int) - Local subscription ID

**Response (200 OK):**

```json
{
  "message": "Subscription synced successfully",
  "subscription": {
    "id": 1,
    "status": "active"
  }
}
```

---

## Invoice Endpoints

### Get Invoice

Retrieves invoice details.

**Endpoint:** `GET /api/invoice/{invoiceId}`

**URL Parameters:**

- `invoiceId` (int) - Local invoice ID

**Response (200 OK):**

```json
{
  "id": 1,
  "stripeInvoiceId": "in_1234567890",
  "customerId": 1,
  "subscriptionId": 1,
  "status": "paid",
  "invoiceNumber": "0001",
  "total": 9999,
  "amountDue": 9999,
  "amountPaid": 9999,
  "currency": "usd",
  "description": "Monthly subscription",
  "dueDate": "2026-06-15T00:00:00Z",
  "issuedAt": "2026-05-30T10:00:00Z",
  "paidAt": "2026-05-30T10:05:00Z",
  "paymentMethod": "paid",
  "pdfUrl": "https://invoice.stripe.com/...",
  "createdAt": "2026-05-30T10:00:00Z",
  "updatedAt": "2026-05-30T10:00:00Z"
}
```

---

### List Customer Invoices

Lists invoices for a customer.

**Endpoint:** `GET /api/invoice/customer/{customerId}`

**URL Parameters:**

- `customerId` (int) - Local customer ID

**Query Parameters:**

- `status` (string, optional) - Filter by status (paid, open, draft, etc.)
- `limit` (int, optional) - Results limit (default: 10)

**Response (200 OK):**

```json
{
  "data": [
    {
      "id": 1,
      "stripeInvoiceId": "in_1234567890",
      "invoiceNumber": "0001",
      "status": "paid",
      "total": 9999,
      "issuedAt": "2026-05-30T10:00:00Z"
    }
  ],
  "count": 1
}
```

---

### List Subscription Invoices

Lists invoices for a subscription.

**Endpoint:** `GET /api/invoice/subscription/{subscriptionId}`

**URL Parameters:**

- `subscriptionId` (int) - Local subscription ID

**Query Parameters:**

- `status` (string, optional) - Filter by status
- `limit` (int, optional) - Results limit (default: 10)

**Response (200 OK):**

```json
{
  "data": [
    {
      "id": 1,
      "stripeInvoiceId": "in_1234567890",
      "status": "paid",
      "total": 9999
    }
  ],
  "count": 1
}
```

---

### Get Invoice PDF

Gets the PDF download URL for an invoice.

**Endpoint:** `GET /api/invoice/{invoiceId}/pdf`

**URL Parameters:**

- `invoiceId` (int) - Local invoice ID

**Response (200 OK):**

```json
{
  "pdfUrl": "https://invoice.stripe.com/..."
}
```

---

### Sync Invoice

Syncs an invoice from Stripe.

**Endpoint:** `POST /api/invoice/{invoiceId}/sync`

**URL Parameters:**

- `invoiceId` (int) - Local invoice ID

**Response (200 OK):**

```json
{
  "message": "Invoice synced successfully",
  "invoice": {
    "id": 1,
    "status": "paid"
  }
}
```

---

### Mark Invoice as Paid

Marks an invoice as paid.

**Endpoint:** `POST /api/invoice/{invoiceId}/mark-paid`

**URL Parameters:**

- `invoiceId` (int) - Local invoice ID

**Response (200 OK):**

```json
{
  "message": "Invoice marked as paid",
  "status": "paid"
}
```

---

### Mark Invoice as Failed

Marks an invoice payment as failed.

**Endpoint:** `POST /api/invoice/{invoiceId}/mark-failed`

**URL Parameters:**

- `invoiceId` (int) - Local invoice ID

**Request Body (optional):**

```json
{
  "reason": "insufficient_funds"
}
```

**Response (200 OK):**

```json
{
  "message": "Invoice marked as failed",
  "status": "payment_failed"
}
```

---

## Webhook Endpoints

### Receive Webhook

Receives and processes Stripe webhook events.

**Endpoint:** `POST /api/webhook/stripe`

**Headers:**

- `Stripe-Signature` (required) - Webhook signature

**Request Body:**
Raw JSON event from Stripe (example):

```json
{
  "id": "evt_1234567890",
  "object": "event",
  "type": "subscription.updated",
  "data": {
    "object": {
      "id": "sub_1234567890",
      "status": "active"
    }
  }
}
```

**Response (200 OK):**

```json
{
  "message": "Webhook processed successfully"
}
```

**Supported Events:**

- `customer.created`
- `customer.updated`
- `customer.deleted`
- `subscription.created`
- `subscription.updated`
- `subscription.deleted`
- `invoice.created`
- `invoice.payment_succeeded`
- `invoice.payment_failed`
- `invoice.finalized`

---

### Webhook Health Check

Health check endpoint.

**Endpoint:** `GET /api/webhook/health`

**Response (200 OK):**

```json
{
  "status": "healthy"
}
```

---

## Error Handling

### Common Errors

**Invalid Customer:**

```json
{
  "error": "Customer not found"
}
```

**Missing Required Field:**

```json
{
  "error": "Email is required"
}
```

**Stripe API Error:**

```json
{
  "error": "Your card was declined"
}
```

**Server Error:**

```json
{
  "error": "Internal server error"
}
```

---

## Rate Limiting

Rate limiting will be implemented in Phase 9. Currently, there are no rate limits.

---

## Pagination

List endpoints support pagination:

**Query Parameters:**

- `page` (int) - Page number (default: 1)
- `limit` (int) - Results per page (default: 10)

**Response:**

```json
{
  "data": [...],
  "pagination": {
    "page": 1,
    "limit": 10,
    "total": 25,
    "pages": 3
  }
}
```

---

## Testing with cURL

### Register Customer

```bash
curl -X POST https://localhost:7001/api/customer/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "name": "Test User"
  }'
```

### Create Subscription

```bash
curl -X POST https://localhost:7001/api/subscription/create \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": 1,
    "stripePriceId": "price_your_price_id"
  }'
```

### List Subscriptions

```bash
curl -X GET "https://localhost:7001/api/subscription/customer/1" \
  -H "Content-Type: application/json"
```

---

## Swagger UI

For interactive API documentation and testing, visit:

```
https://localhost:7001/swagger
```

---

**Last Updated**: 2026-05-30
**API Version**: 1.0.0
