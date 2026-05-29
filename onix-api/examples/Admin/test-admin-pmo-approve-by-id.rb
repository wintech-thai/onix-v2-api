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
pmrId = 'c0585dd9-902c-400c-a91c-ae79f10204a2'
paymentBankAccountId = '57e57db4-b583-405d-bd13-b21c93997524'

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
