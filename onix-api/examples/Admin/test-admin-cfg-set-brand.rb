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
storageApiBase = "https://storage-api.please-payment.com"
fileUpload = 'logo.svg'
mimeType = "image/svg+xml"
hhmmss = Time.now.strftime("%H%M%S")

### 
apiUrl = "admin-api/AdminConfiguration/org/global/action/GetBrandLogoUploadPresignedUrl"
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

#puts("Url : #{presignedUrl}")
#puts("Object Name : #{objectName}")
###

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

### 
apiUrl = "admin-api/AdminConfiguration/org/global/action/SetBrandConfig"
param = {
  Status: "Enable",
  BrandConfig: {
    BrandName: "Test Brand",
    LogoPath: objectName,
    ThemeName: "GREEN_NATURAL",
  }
}

result = make_request(:post, apiUrl, param)
puts(result.to_json)
