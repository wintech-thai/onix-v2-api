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

###
apiUrl = "admin-api/AdminCurrencyAccount/org/global/action/AddCurrencyAccount"
param =  {
  Currency: "KAS",
  CurrencyName: "Kaspa",
  CurrencyCategory: "CRYPTO",
  AccountType: "PayIn",
  AccountLevel: "Global",
  Tags: "Testing",

  CryptoWalletNetwork: "KASPA",
  CryptoWalletType: "HD",
  CryptoDerivationPath: "m/44'/111111'/0'/0",
  CryptoQrScheme: "KASPA",
  CryptoAddressPrefix: "kaspa:",
  CryptoDecimal: 8,
  CryptoExtendedPublicKey: "xpub-test-#{Time.now.to_i}",
  CryptoAddressBranch: 0,

  TxMinAmount: 1,
  TxMaxAmount: 1000000,
  DailyTotalAmountLimit: 5000000,
  DailyTotalCountLimit: 1000,
}

token = File.read(keyFile)

ENV['API_KEY'] = nil # ถ้าไม่ใช้ API KEY ก็เซ็ตเป็น nil
ENV['ACCESS_TOKEN'] = token

result = make_request(:post, apiUrl, param)
puts(result)
