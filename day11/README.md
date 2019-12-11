# Day 11 - Database Trigger

A system that takes in childrens' requests and stores them for later processing. Additionally, a notification is sent every time a new gift wish arrives.

Bonus: A webpage with a form that submits data in the previous format!

## Solution

Azure Functions 3.0 using .Net Core 3.1 with C#. HTTP triggered functions leveraging output bindings using Azure Queue Storage. A queue storage triggered function that saves the message to Azure Table Storage and sends a message to Teams when a new row is added.

* Present a form page to allow a child to submit a wish
  * \[GET\] /
* Accept a wish and queue for processing
 * \[POST\] /api/wish

## Requirements

* Requires an Azure Storage Table and and Azure Storage Queue to be setup
* Requires a webhook to be set up on a teams channel and the URL placed in the function application's settings
