@echo off
echo ======================================
echo    Hotel Booking System - SignalR Test
echo ======================================
echo.
echo Starting test for real-time notifications...
echo.

echo [1/4] Starting server...
start "HotelServer" cmd /k "cd /d d:\FULearning\Ky8\PRN222\HotelBookingSystem\HotelBookingSystem && dotnet run"

echo [2/4] Waiting for server to start...
timeout /t 8

echo [3/4] Opening browsers for testing...
echo Opening Chrome for ADMIN (admin@hoteltest.com / Test@123)
start chrome --new-window "https://localhost:7001" --user-data-dir="%temp%\chrome-admin"

timeout /t 3

echo Opening Edge for CUSTOMER (test.customer@example.com / Test@123)  
start msedge --new-window "https://localhost:7001" --user-data-dir="%temp%\edge-customer"

echo.
echo [4/4] TEST INSTRUCTIONS:
echo =====================================
echo 1. CHROME (Admin):
echo    - Login: admin@hoteltest.com / Test@123
echo    - Open F12 Console 
echo    - Look for: [DEBUG] Added to AdminGroup
echo.
echo 2. EDGE (Customer):  
echo    - Login: test.customer@example.com / Test@123
echo    - Open F12 Console
echo    - Book a room
echo.
echo 3. EXPECTED RESULTS:
echo    - Customer: Gets booking confirmation notification
echo    - Admin: Gets new booking notification  
echo    - Both: Icon bell shows red badge with count
echo.
echo Press any key to stop all servers...
pause

echo Stopping servers...
taskkill /f /fi "WINDOWTITLE eq HotelServer*" 2>nul
echo Done!
