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
pmrId = '034158d1-b8a2-42d6-9278-0b0c06c3bfe5'

### 
apiUrl = "admin-api/AdminPaymentRequest/org/global/action/RejectPendingPayInRequestById/#{pmrId}"
param = {
  StatusReason: "Testing to reject",
}

token = File.read(keyFile)

ENV['API_KEY'] = nil # ถ้าไม่ใช้ API KEY ก็เซ็ตเป็น nil
ENV['ACCESS_TOKEN'] = token

#puts("===[#{token}]")

result = make_request(:post, apiUrl, param)
puts(result.to_json)
