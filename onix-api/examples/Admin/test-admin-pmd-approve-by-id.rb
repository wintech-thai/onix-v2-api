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
pmdId = 'c89bd18d-df5a-4d71-b1ec-35814dc0a4c0'

### 
apiUrl = "admin-api/AdminPaymentDocument/org/global/action/ApprovePayInDocumentById/#{pmdId}"
param = {
  TxAmountDecimal: 100.00,
  TxAmount: 100.00,
  Currency: "THB",
  RefId: "TestRefId-#{Time.now.to_i}",
  PayInBankAccountId: '35c050b6-3015-407c-8a0a-4f8a35eb8944',
  MerchantId: 'cfd098d5-ed0b-4acd-a10e-9fbb9e3d42c0',
}

token = File.read(keyFile)

ENV['API_KEY'] = nil # ถ้าไม่ใช้ API KEY ก็เซ็ตเป็น nil
ENV['ACCESS_TOKEN'] = token

#puts("===[#{token}]")

result = make_request(:post, apiUrl, param)
puts(result)
