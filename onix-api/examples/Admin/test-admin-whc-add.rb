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
apiUrl = "admin-api/AdminWebhookConfig/org/global/action/AddMerchantWebhookConfig/#{orgId}/#{merchantId}"
param = {
  EventName: "Payment.Success2",
  Description: "Test#1",
  EndpointUrl: "https://hook.please-payment.com",
  HttpMethod: "POST",
  IsActive: true,
  SecretKey: "This is secret",
  SignatureAlgorithm: "HMAC-SHA256",
  TimeoutSec: 2,
  MaxRetryCount: 1,
  RetryIntervalSec: 10,

  Headers: {
    XXXXXX: "ssdfsdf"
  }
}

token = File.read(keyFile)

ENV['API_KEY'] = nil # ถ้าไม่ใช้ API KEY ก็เซ็ตเป็น nil
ENV['ACCESS_TOKEN'] = token

#puts("===[#{token}]")

result = make_request(:post, apiUrl, param)
puts(result)
