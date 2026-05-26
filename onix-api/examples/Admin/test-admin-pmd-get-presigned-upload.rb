#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = ENV['API_ORG']
keyFile = ".token"
merchantId = 'cfd098d5-ed0b-4acd-a10e-9fbb9e3d42c0' # ppm-alfa999
storageApiBase = "https://storage-api.please-payment.com"
fileUpload = 'S__196853762.jpg'
mimeType = "image/jpeg"

hhmmss = Time.now.strftime("%H%M%S")

### 
apiUrl = "admin-api/AdminPaymentDocument/org/global/action/GetPayInSlipUploadPresignedUrl/#{merchantId}"
param =  {
  MimeType: mimeType,
}

token = File.read(keyFile)

ENV['API_KEY'] = nil # ถ้าไม่ใช้ API KEY ก็เซ็ตเป็น nil
ENV['ACCESS_TOKEN'] = token

#puts("===[#{token}]")

result = make_request(:post, apiUrl, param)

presignedUrl = result["presignedUrl"]
objectName = result["objectName"]

#puts(presignedUrl)
#puts(objectName)

##### Replace ค่า <STORAGE-API-BASE> ด้วย storageApiBase
actualUrl = presignedUrl.gsub("<STORAGE-API-BASE>", storageApiBase)
puts("##### ActualUrl: #{actualUrl}")
puts("##### ObjectName: #{objectName}")

##### ตรงนี้คือสำหรับ upload file
uri = URI.parse(actualUrl)
http = Net::HTTP.new(uri.host, uri.port)
http.use_ssl = (uri.scheme == "https")

request = Net::HTTP::Put.new(uri)

# ต้องตรงกับ MimeType ที่ใช้ตอนขอ Presigned URL
request["Content-Type"] = mimeType

File.open(fileUpload, "rb") do |file|
  request.body = file.read
end

response = http.request(request)
puts("Upload Status: #{response.code}")
puts(response.body)

if response.is_a?(Net::HTTPSuccess)
  puts("Upload success")
else
  puts("Upload failed")
end

#### เรียก API อีกครั้ง
apiUrl = "admin-api/AdminPaymentDocument/org/global/action/AddPayInDocument/#{merchantId}"
param =  {
  UploadedFilePath: objectName,
  MimeType: mimeType,
  TxAmountDecimal: 100.00,
  PayInBankAccountId: '35c050b6-3015-407c-8a0a-4f8a35eb8944',
  MerchantId: 'cfd098d5-ed0b-4acd-a10e-9fbb9e3d42c0',
  RefId: "TestRefId-#{Time.now.to_i}",
}

result = make_request(:post, apiUrl, param)
puts(result)
