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
pmrId = '5d335b0c-1d2f-43ac-9df1-b6d2249d68f8' # ppm-alfa999
srcPaymentBankAccountId = 'f1b4334b-a689-4adc-a9eb-b489c18fcbab' # transit bank account

hhmmss = Time.now.strftime("%H%M%S")

### 
apiUrl = "admin-api/AdminPaymentRequest/org/global/action/UpdateTransferRequestById/#{pmrId}"
param =  {
  PayoutBankAccountId: "#{srcPaymentBankAccountId}",
  #SelectedPayInBankAccountId:
}

token = File.read(keyFile)

ENV['API_KEY'] = nil # ถ้าไม่ใช้ API KEY ก็เซ็ตเป็น nil
ENV['ACCESS_TOKEN'] = token

#puts("===[#{token}]")

result = make_request(:post, apiUrl, param)
puts(result.to_json)
