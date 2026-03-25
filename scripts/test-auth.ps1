Write-Host "ZIVRO AUTHENTICATION TEST" -ForegroundColor Cyan

Add-Type -AssemblyName System.Net.Http

$baseUrl = "https://localhost:7249"
$handler = New-Object System.Net.Http.HttpClientHandler
$handler.ServerCertificateCustomValidationCallback = { $true }
$client = New-Object System.Net.Http.HttpClient($handler)

# TEST 1: REGISTER
Write-Host "
TEST 1: REGISTER" -ForegroundColor Magenta
$registerJson = @{
    email = "testuser@example.com"
    name = "Test User"
    password = "Password@123"
} | ConvertTo-Json -Compress

$request = New-Object System.Net.Http.HttpRequestMessage
$request.Method = [System.Net.Http.HttpMethod]::Post
$request.RequestUri = "$baseUrl/api/auth/register"
$request.Content = New-Object System.Net.Http.StringContent($registerJson, [System.Text.Encoding]::UTF8, "application/json")

$response = $client.SendAsync($request).Result
$content = $response.Content.ReadAsStringAsync().Result
$data = $content | ConvertFrom-Json

Write-Host "Status: $($response.StatusCode)" -ForegroundColor Green
Write-Host "User ID: $($data.userId)" -ForegroundColor Yellow
Write-Host "Token: $($data.accessToken.Substring(0, 50))..." -ForegroundColor Yellow

$token = $data.accessToken

# TEST 2: LOGIN
Write-Host "
TEST 2: LOGIN" -ForegroundColor Magenta
$loginJson = @{
    email = "testuser@example.com"
    password = "Password@123"
} | ConvertTo-Json -Compress

$request2 = New-Object System.Net.Http.HttpRequestMessage
$request2.Method = [System.Net.Http.HttpMethod]::Post
$request2.RequestUri = "$baseUrl/api/auth/login"
$request2.Content = New-Object System.Net.Http.StringContent($loginJson, [System.Text.Encoding]::UTF8, "application/json")

$response2 = $client.SendAsync($request2).Result
$content2 = $response2.Content.ReadAsStringAsync().Result
$data2 = $content2 | ConvertFrom-Json

Write-Host "Status: $($response2.StatusCode)" -ForegroundColor Green
Write-Host "User ID: $($data2.userId)" -ForegroundColor Yellow

# TEST 3: AUTHORIZED REQUEST
Write-Host "
TEST 3: AUTHORIZED REQUEST (Audit Logs)" -ForegroundColor Magenta
$request3 = New-Object System.Net.Http.HttpRequestMessage
$request3.Method = [System.Net.Http.HttpMethod]::Get
$request3.RequestUri = "$baseUrl/api/auth/audit-logs"
$request3.Headers.Add("Authorization", "Bearer $token")

$response3 = $client.SendAsync($request3).Result
$content3 = $response3.Content.ReadAsStringAsync().Result
$data3 = $content3 | ConvertFrom-Json

Write-Host "Status: $($response3.StatusCode)" -ForegroundColor Green
Write-Host "Audit logs count: $($data3.logs.Count)" -ForegroundColor Yellow

# TEST 4: UNAUTHORIZED REQUEST
Write-Host "
TEST 4: UNAUTHORIZED REQUEST (No Token)" -ForegroundColor Magenta
$request4 = New-Object System.Net.Http.HttpRequestMessage
$request4.Method = [System.Net.Http.HttpMethod]::Get
$request4.RequestUri = "$baseUrl/api/auth/audit-logs"

$response4 = $client.SendAsync($request4).Result
Write-Host "Status: $($response4.StatusCode)" -ForegroundColor 

Write-Host "
ALL TESTS COMPLETED" -ForegroundColor Cyan
