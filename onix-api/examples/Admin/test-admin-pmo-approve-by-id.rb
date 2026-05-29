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
pmrId = '0529bba7-ad0a-4729-93c3-c54c5612c088'
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
