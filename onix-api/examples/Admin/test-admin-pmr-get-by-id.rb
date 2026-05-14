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

### 
apiUrl = "admin-api/AdminPaymentRequest/org/global/action/GetPayInRequests"
param =  {
  FullTextSearch: "",
  Direction: "", #"PayIn",
  Status: "",
}

token = File.read(keyFile)

ENV['API_KEY'] = nil # ถ้าไม่ใช้ API KEY ก็เซ็ตเป็น nil
ENV['ACCESS_TOKEN'] = token

#puts("===[#{token}]")

result = make_request(:post, apiUrl, param)
puts(result)

apiUrl = "admin-api/AdminPaymentRequest/org/global/action/GetPayInRequestCount"

result = make_request(:post, apiUrl, param)
puts(result)
