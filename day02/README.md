# Day 2 - Task Scheduler

https://25daysofserverless.com/calendar/2

Task scheduler that will tell Lucy exactly when she should relight candles, pour coffee into cups, and deliver batches of coffee.

## Solution

Azure Timer triggered Durable Function that runs annually on a specifed date and time and uses Durable Timers in between tasks to control flow.