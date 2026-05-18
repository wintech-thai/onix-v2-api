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
bankAccountId = 'cfd098d5-ed0b-4acd-a10e-9fbb9e3d42c0' #'cfd098d5-ed0b-4acd-a10e-9fbb9e3d42c0'

hhmmss = Time.now.strftime("%H%M%S")

### 
apiUrl = "admin-api/AdminPaymentTx/org/global/action/SubmitLinePaymentTxNotification/#{bankAccountId}"
param =  {
  PaymentAmount: 123.89,
  RemainAmount: 0.00,
  TxType: "PayIn",
  SourceBankCode: "KTB",
  SourceBankAccountNo: "XX-0987",
  DestinationBankCode: "TMB",
  DestinationAccountNo: "XX-9032",
}

token = File.read(keyFile)

ENV['API_KEY'] = nil # ถ้าไม่ใช้ API KEY ก็เซ็ตเป็น nil
ENV['ACCESS_TOKEN'] = token

#puts("===[#{token}]")

result = make_request(:post, apiUrl, param)
puts(result)
