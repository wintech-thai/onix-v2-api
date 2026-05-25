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

hhmmss = Time.now.strftime("%H%M%S")

### 
apiUrl = "admin-api/AdminPaymentDocument/org/global/action/GetPayInSlipUploadPresignedUrl/#{merchantId}"
param =  {
  MimeType: "image/png",
}

token = File.read(keyFile)

ENV['API_KEY'] = nil # ถ้าไม่ใช้ API KEY ก็เซ็ตเป็น nil
ENV['ACCESS_TOKEN'] = token

#puts("===[#{token}]")

result = make_request(:post, apiUrl, param)
puts(result)
