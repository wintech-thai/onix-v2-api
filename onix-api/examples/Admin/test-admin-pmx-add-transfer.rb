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
merchantId = 'cfd098d5-ed0b-4acd-a10e-9fbb9e3d42c0' # ppm-alfa999
transitPaymentBankAccountId = 'e151b253-8fbc-4a4f-9402-72c242919b15' # transit bank account
srcPaymentBankAccountId = 'f1b4334b-a689-4adc-a9eb-b489c18fcbab' # transit bank account

hhmmss = Time.now.strftime("%H%M%S")

### 
apiUrl = "admin-api/AdminPaymentRequest/org/global/action/CreateTransferRequest"
param =  {
  RefId: "XA-MSB-0001922-#{hhmmss}",
  RefId1: "INVOICE-0011223",
  RefId2: "ORDER-XSWKEKEI",
  Description: "ทดสอบยิง payment transfer เข้ามาเฉย ๆ",
  Currency: "THB",
  RequestedAmount: 3000.43,
  QrProvider: "PP",
  Tags: "testing",

  PayinBankAccountId: "#{transitPaymentBankAccountId}",
  PayoutBankAccountId: "#{srcPaymentBankAccountId}",
  #SelectedPayInBankAccountId:
}

token = File.read(keyFile)

ENV['API_KEY'] = nil # ถ้าไม่ใช้ API KEY ก็เซ็ตเป็น nil
ENV['ACCESS_TOKEN'] = token

#puts("===[#{token}]")

result = make_request(:post, apiUrl, param)
puts(result.to_json)
