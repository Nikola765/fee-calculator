# Fee Calculation Service

## Overview
Fee calculation engine built with clean architecture principles, featuring a flexible rule-based tariff system for payment transactions.

## Getting Started
Before you start, make sure you have the following prerequisites installed: 
- .NET 8
- VSCode IDE (or some other IDE)

To get started, follow this instructions for local setup:
1. Clone this repository to your local machine.
2. Navigate to the cloned directory.
3. Restore dependencies for all projects
`dotnet restore`
4. Build the entire solution
`dotnet build`
5. Start the API (Terminal 1)
`cd FeeCalculator.Api`
`dotnet run`

## Sample data and test
API will be available at: `http://localhost:5062`

### Test the transaction with hardcoded transactions that cover each rule from the task OR with random transactions:
1. Send a GET request to the `/api/TestData/scenarios` endpoint to retrieve the hardcoded transactions OR to the `/api/TestData/transactions` endpoint to retrieve randomly generated transactions
2. Copy the desired transaction from the response body from the GET request.
3. Send a POST request to the `/api/FeeCalculator/calculate endpoint`.

Note: 
Example body for POST `/api/FeeCalculator/calculate`

`{
    "transactionId": "POS_UNDER_100_01",
    "transactionType": "POS",
    "amount": 50,
    "currency": "EUR",
    "isDomestic": true,
    "merchantCategory": "GROCERY",
    "channel": "POS",
    "isRecurring": false,
    "transactionDate": "2025-07-27T19:27:19.231902Z",
    "client": {
      "clientId": "TEST_CLIENT_01",
      "creditScore": 350,
      "clientSegment": "STANDARD",
      "hasActivePromotions": false,
      "clientSince": "2025-01-27T19:27:19.231904Z",
      "monthlyVolume": 1000,
      "transactionCountThisMonth": 10,
      "businessType": "INDIVIDUAL",
      "riskLevel": "MEDIUM",
      "activePromotions": [],
      "additionalAttributes": {}
    },
    "additionalAttributes": {}
}`

### Test the performance:
1. Send a GET request to the `/api/TestData/performance-batch` endpoint to retrieve test data for performance.
2. Copy the response body from the GET request.
3. Paste the copied response body into the request body and send a POST request to the `/api/FeeCalculator/calculate-batch` endpoint.

### Test updating rule status:
1. Send a PUT request to the `/api/RuleManagement/processors/{ruleId}/status` endpoint with the following example parameters:
- `ruleId = 4`
- `isActive = true`

### Test calculation history for transactions:
1. After any calculation, you can get the history of transactions at the following endpoint: `/api/FeeCalculator/history`.

Note: I have a bug here but I didn't have time to investigate and resolve it.

## Decisions/Compromises
Some of the decisions/compromises that I had to make during this task:
- Define language, framework and libraries: I chose to build this service with C#/.NET because it handles high volumes of requests efficiently (which makes it ideal for batch processing), strong security features, large and active community, etc.
- Design the architecture: I followed clean architecture principles like clear layer boundaries, testability, scalability, and etc.

## What would follow if I had more time?
If I had more time, I would:
- More detailed testing and resolving potential issues (already found some)
- Review the code and optimize it (if possible)
- Prepare minimal UI