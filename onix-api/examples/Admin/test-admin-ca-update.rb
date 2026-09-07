#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = "global"
keyFile = ".token"
currencyAccountId = 'REPLACE-WITH-ID-FROM-ADD-RESULT'

###
apiUrl = "admin-api/AdminCurrencyAccount/org/#{orgId}/action/UpdateCurrencyAccountById/#{currencyAccountId}"
param = {
  AccountLevel: "Selected",
  Tags: "Testing,Updated",
  CryptoDerivationPath: "m/44'/111111'/0'/1",
  CryptoQrScheme: "KASPA",
  CryptoAddressPrefix: "kaspa:",
  CryptoDecimal: 8,
  CryptoAddressBranch: 0,
  TxMinAmount: 5,
  TxMaxAmount: 2000000,
  DailyTotalAmountLimit: 10000000,
  DailyTotalCountLimit: 2000,
  IsRandomCent: false,
  DecimalAction: "Round",
}

token = File.read(keyFile)

ENV['API_KEY'] = nil # ถ้าไม่ใช้ API KEY ก็เซ็ตเป็น nil
ENV['ACCESS_TOKEN'] = token

result = make_request(:post, apiUrl, param)
puts(result)
