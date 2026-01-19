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
ENV['API_KEY'] = nil # ถ้าไม่ใช้ API KEY ก็เซ็ตเป็น nil

apiUrl = "customer-api/AuthCustomer/org/#{orgId}/action/Login"
param =  {
  UserName: "#{ENV['CUST_USER_NAME']}",
  Password: "#{ENV['USER_PASSWORD']}",
}

result = make_request(:post, apiUrl, param)
token = result["token"]["access_token"]
puts(token)
#puts("======")

#apiUrl = "customer-api/AuthCustomer/org/#{orgId}/action/Refresh"
#param =  {
#  RefreshToken: result["token"]["refresh_token"],
#}
#result = make_request(:post, apiUrl, param)

#token = result["token"]["access_token"]
#puts(token)

File.write(keyFile, token)
