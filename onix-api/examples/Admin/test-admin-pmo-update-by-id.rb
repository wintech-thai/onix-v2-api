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
pmrId = '9fc253a8-fa6f-4a08-84d2-c459d6523c27'
paymentBankAccountId = '57e57db4-b583-405d-bd13-b21c93997524'
merchantId = 'cfd098d5-ed0b-4acd-a10e-9fbb9e3d42c0' # ppm-alfa999

### 
apiUrl = "admin-api/AdminPaymentRequest/org/global/action/UpdatePayOutRequestById/#{pmrId}"
param = {
  PayoutBankAccountId: "#{paymentBankAccountId}",
  MerchantId: "#{merchantId}",
}

token = File.read(keyFile)

ENV['API_KEY'] = nil # ถ้าไม่ใช้ API KEY ก็เซ็ตเป็น nil
ENV['ACCESS_TOKEN'] = token

#puts("===[#{token}]")

result = make_request(:post, apiUrl, param)
puts(result)
