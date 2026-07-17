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
pmtId = '9409a351-8c24-4f02-8943-448fd67829e2'
merchantId = 'eab2eae2-ab83-4d49-bff6-a30226663d09'

### 
apiUrl = "admin-api/AdminPaymentTx/org/global/action/ApproveUnidentifiedPaymentTx/#{pmtId}/#{merchantId}"
param = nil

token = File.read(keyFile)

ENV['API_KEY'] = nil # ถ้าไม่ใช้ API KEY ก็เซ็ตเป็น nil
ENV['ACCESS_TOKEN'] = token

#puts("===[#{token}]")

result = make_request(:post, apiUrl, param)
puts(result.to_json)
