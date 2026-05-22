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
paymentBankAccountId = ''

hhmmss = Time.now.strftime("%H%M%S")

### 
apiUrl = "admin-api/AdminPaymentRequest/org/global/action/CreatePayOutRequest"
param =  {
  RefId: "XA-MSB-0001922-#{hhmmss}",
  RefId1: "INVOICE-0011223",
  RefId2: "ORDER-XSWKEKEI",
  Description: "ทดสอบยิง payment request เข้ามาเฉย ๆ",
  Currency: "THB",
  RequestedAmount: 325,
  QrProvider: "PP",
  Tags: "testing",

  PayinBankAccountId: "#{paymentBankAccountId}",
  MerchantId: "#{merchantId}",
  #SelectedPayInBankAccountId:
}

token = File.read(keyFile)

ENV['API_KEY'] = nil # ถ้าไม่ใช้ API KEY ก็เซ็ตเป็น nil
ENV['ACCESS_TOKEN'] = token

#puts("===[#{token}]")

result = make_request(:post, apiUrl, param)
puts(result)
