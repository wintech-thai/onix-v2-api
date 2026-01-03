#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require 'cgi'
require 'base64'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

url = "https://register-dev.please-scan.com/napbiotec/customer-forgot-password/bb1bdb8e-6085-4633-a568-911128cdc0e6?data=eyJFbWFpbCI6InBqYW1lLmZiQGdtYWlsLmNvbSIsIlVzZXJOYW1lIjoicGphbWUuZmJAZ21haWwuY29tIiwiUGFzc3dvcmQiOm51bGwsIk5hbWUiOm51bGwsIkxhc3RuYW1lIjpudWxsLCJJbnZpdGVkQnkiOm51bGwsIk9yZ1VzZXJJZCI6IjM0MmM5NWRiLTZmNDgtNDU5Zi05Zjc0LWYwZWZmNmVlYTcxYyJ9"
uri = URI.parse(url)

# แปลง query string เป็น hash
params = CGI.parse(uri.query)
data = params['data'].first

path = uri.path
parts = path.split('/').reject(&:empty?)

decoded = Base64.decode64(data)
dataObj = JSON.parse(decoded)

orgId = parts[0]
regType = parts[1]
token = parts[2]

api = "ConfirmCustomerForgotPasswordReset"

#puts(dataObj)
customerId = dataObj['OrgUserId']

param =  {
  Email: dataObj['Email'],
  UserName: dataObj['Email'],
  Password: "#{ENV['USER_PASSWORD']}",
  OrgUserId: dataObj['OrgUserId'],
}

ENV['API_KEY'] = nil # ถ้าไม่ใช้ API KEY ก็เซ็ตเป็น nil

apiUrl = "api/Registration/org/#{orgId}/action/#{api}/#{token}/#{customerId}"
#puts(apiUrl)

result = make_request(:post, apiUrl, param)
puts(result)
