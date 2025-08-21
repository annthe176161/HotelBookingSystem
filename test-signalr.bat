@echo off
echo Starting Hotel Booking System for SignalR Testing...

echo.
echo Starting instance 1 on port 7001...
start "Hotel-Port-7001" cmd /k "cd /d d:\FULearning\Ky8\PRN222\HotelBookingSystem\HotelBookingSystem && dotnet run"

timeout /t 5

echo.
echo Starting instance 2 on port 7002...  
start "Hotel-Port-7002" cmd /k "cd /d d:\FULearning\Ky8\PRN222\HotelBookingSystem\HotelBookingSystem && dotnet run --urls=https://localhost:7002;http://localhost:5002"

timeout /t 3

echo.
echo Opening browsers...
start chrome "https://localhost:7001"
timeout /t 2
start msedge "https://localhost:7001" 

echo.
echo Test Instructions:
echo 1. Chrome: Login as Admin (admin@hoteltest.com / Test@123)
echo 2. Edge: Login as Customer (test.customer@example.com / Test@123)  
echo 3. Customer: Book a room
echo 4. Admin: Check for real-time notifications
echo.
echo Press any key to close all instances...
pause

taskkill /f /fi "WINDOWTITLE eq Hotel-Port-*"
