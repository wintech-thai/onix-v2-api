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
merchantId = "cfd098d5-ed0b-4acd-a10e-9fbb9e3d42c0" 
apiUrl = "admin-api/AdminMerchant/org/global/action/UpdateMerchantById/#{merchantId}"
param = {
    Code: "ppm-alfa999",
    Name: "ALPHA-999",
    ContactEmail: "ningnong999@gmail.com",
    ContactPhone: "099-999-9999",
    PayinFeePct: "2",
    PayoutFeePct: "1",
    PayinMinAmount: "300",
    PayinMaxAmount: "50000",
    PayoutMinAmount: "300",
    PayoutMaxAmount: "50000",
}

token = File.read(keyFile)

ENV['API_KEY'] = nil # ถ้าไม่ใช้ API KEY ก็เซ็ตเป็น nil
ENV['ACCESS_TOKEN'] = token

#puts("===[#{token}]")

result = make_request(:post, apiUrl, param)
puts(result)
