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
walletId = '685928ab-d80c-454c-ba8b-0ef3f99f18b6' # ได้มาจาก GetWalletByMerchantId() 
orgId = 'ppm-alfa888' # ได้มาจาก GetWalletByMerchantId() 

### 
apiUrl = "admin-api/AdminWallet/org/global/action/GetPointTxsByWalletId/#{orgId}/#{walletId}"
param = {
  #Offset: 0,
  #Limit: 999,
}

token = File.read(keyFile)

ENV['API_KEY'] = nil # ถ้าไม่ใช้ API KEY ก็เซ็ตเป็น nil
ENV['ACCESS_TOKEN'] = token

#puts("===[#{token}]")

result = make_request(:post, apiUrl, param)
puts(result)

puts("===")

apiUrl = "admin-api/AdminWallet/org/global/action/GetPointTxsCountByWalletId/#{orgId}/#{walletId}"
result = make_request(:post, apiUrl, param)
puts(result)
