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
pmrId = '3e224b33-7cae-4a3f-9d5d-9af6b9b3eb93'
paymentBankAccountId = '35c050b6-3015-407c-8a0a-4f8a35eb8944'

token = File.read(keyFile)

ENV['API_KEY'] = nil # ถ้าไม่ใช้ API KEY ก็เซ็ตเป็น nil
ENV['ACCESS_TOKEN'] = token

### 
apiUrl = "admin-api/AdminPaymentRequest/org/global/action/ApprovePayOutRequestById/#{pmrId}"
param = {
  PayoutBankAccountId: "#{paymentBankAccountId}",
}

result = make_request(:post, apiUrl, param)
puts(result.to_json)
