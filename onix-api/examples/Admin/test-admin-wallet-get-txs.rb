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
merchantId = 'b4f4761e-b745-4bc8-a9d5-59e1ae7080b2' # ได้มาจาก GetWalletByMerchantId() 
orgId = 'ppm-alfa999' # ได้มาจาก GetWalletByMerchantId() 

### 
apiUrl = "admin-api/AdminWallet/org/global/action/GetPointTxsByWalletId/#{orgId}/#{merchantId}"
param = {
  Offset: 1,
  Limit: 10,
}

token = File.read(keyFile)

ENV['API_KEY'] = nil # ถ้าไม่ใช้ API KEY ก็เซ็ตเป็น nil
ENV['ACCESS_TOKEN'] = token

#puts("===[#{token}]")

result = make_request(:post, apiUrl, param)
puts(result)

puts("===")

apiUrl = "admin-api/AdminWallet/org/global/action/GetPointTxsCountByWalletId/#{orgId}/#{merchantId}"
result = make_request(:post, apiUrl, param)
puts(result)
