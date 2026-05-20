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
merchantId = 'cfd098d5-ed0b-4acd-a10e-9fbb9e3d42c0'
orgId = 'ppm-alfa999'

### 
apiUrl = "admin-api/AdminWebhookConfig/org/global/action/GetWebhookConfigsByMerchantId/#{orgId}/#{merchantId}"
param = nil
token = File.read(keyFile)

ENV['API_KEY'] = nil # ถ้าไม่ใช้ API KEY ก็เซ็ตเป็น nil
ENV['ACCESS_TOKEN'] = token

#puts("===[#{token}]")

result = make_request(:get, apiUrl, param)
puts(result)

# ให้เอา orgId กับ walletId เพื่อไปใช้ต่อในการเรียก API อื่น ๆ

puts("===============")
apiUrl = "admin-api/AdminWebhookConfig/org/global/action/GetWebhookConfigsCountByMerchantId/#{orgId}/#{merchantId}"
result = make_request(:get, apiUrl, param)
puts(result)
