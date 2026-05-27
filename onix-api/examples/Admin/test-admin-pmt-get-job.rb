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
pmtId = 'c7f1b284-e21b-489a-a9b9-21c69ddc4386'
jobId = 'fd3fa695-fb3d-4276-9441-0a8da4811d43'

### 
apiUrl = "admin-api/AdminPaymentTx/org/global/action/GetPaymentTransactionJobById/#{pmtId}/#{jobId}"
param = nil

token = File.read(keyFile)

ENV['API_KEY'] = nil # ถ้าไม่ใช้ API KEY ก็เซ็ตเป็น nil
ENV['ACCESS_TOKEN'] = token

#puts("===[#{token}]")

result = make_request(:get, apiUrl, param)
puts(result.to_json)
