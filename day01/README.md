# Day 1 - A Basic Function

https://25daysofserverless.com/calendar/1

REST API endpoint that spins a dreidel and randomly returns נ (Nun), ג (Gimmel), ה (Hay), or ש (Shin).

## Solution

Azure HTTP triggered Function that accepts a request for a spin result.
* \[GET\] - Returns 200 OK along with the result of the spin.