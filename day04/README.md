# Day 4 - API Endpoint

https://25daysofserverless.com/calendar/4

An HTTP API that lets Ezra's friends add food dishes they want to bring to the potluck, change or remove them if they need to (plans change!), and see a list of what everybody's committed to bring.

## Solution

Azure HTTP triggered functions with a table storage back end

* http://localhost:7071/api/potluck/{potluckId}/dishes
  * \[GET\] - Get a list of dishes for a potluck.
    * Returns 200 OK with a list of objects.
  * \[POST\] - Add a dish for a potluck.
    * Returns 202 Accepted with location header and the object.
    * Returns 400 Bad Request if name or email are null or whitespace.
  * Any other request method
    * Returns 404 Not Found if the request method is not in the function declaration
    * Returns 405 Method Not Allowed if the developer makes a mistake and adds a request method in the function declaration without an implementation.

* http://localhost:7071/api/potluck/{potluckId}/dishes/{dishId}
  * \[GET\] - Get a dish for a potluck by the dish's id.
    * Returns 200 OK with the object
    * Returns 404 Not Found if the record for the potluck id and dish id was not found or if the table has not yet been created.
  * \[PATCH\] - Update a dish for a potluck by the dish's id.
    * Returns 202 Accepted with location header and the object.
    * Returns 400 Bad Request if name or email are null or whitespace.
    * Returns 404 Not Found if the record for the potluck id and dish id was not found or if the table has not yet been created.
  * \[POST\] - Delete a dish for a potluck by the dish's id.
    * Returns 202 Accepted.
    * Returns 404 Not Found if the record for the potluck id and dish id was not found or if the table has not yet been created.
  * Any other request method
    * Returns 404 Not Found if the request method is not in the function declaration
    * Returns 405 Method Not Allowed if the developer makes a mistake and adds a request method in the function declaration without an implementation.
