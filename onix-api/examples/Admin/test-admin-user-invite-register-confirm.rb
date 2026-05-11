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

url = "https://web-dev.please-payment.com/admin-signup-confirm/global/6f42e74e-e0c5-448a-a8a5-369b7e78338e?data=eyJFbWFpbCI6ImhlbGxvLnNldWJAYWJjZGVmZy5jb20iLCJVc2VyTmFtZSI6InNldWJwb25nLnNvb2Rsb3IiLCJQYXNzd29yZCI6bnVsbCwiTmFtZSI6bnVsbCwiTGFzdG5hbWUiOm51bGwsIkludml0ZWRCeSI6InNldWJwb25nLm1vbiIsIk9yZ1VzZXJJZCI6IjE0MTg2MTNiLTA3MmUtNDE2MC04ZmUwLWQ1YjVmNjNlMmE2NSIsIk9yZ1R5cGUiOm51bGx9"
uri = URI.parse(url)

# แปลง query string เป็น hash
params = CGI.parse(uri.query)
data = params['data'].first

path = uri.path
parts = path.split('/').reject(&:empty?)

decoded = Base64.decode64(data)
dataObj = JSON.parse(decoded)

orgId = parts[1]
regType = parts[0]
token = parts[2]

api = "ConfirmNewUserInvitation"
if (regType != 'admin-signup-confirm')
  api = "ConfirmExistingUserInvitation"
end

#puts(dataObj)
userName = dataObj['UserName']

param =  {
  Email: dataObj['Email'],
  UserName: "#{userName}",
  Password: "Abc12345$343#1",
  Name: "PJames",
  LastName: "Soodlor",
  OrgUserId: dataObj['OrgUserId'],
}

#puts(param)
#puts(parts)

ENV['API_KEY'] = nil # ถ้าไม่ใช้ API KEY ก็เซ็ตเป็น nil
ENV['ACCESS_TOKEN'] = nil

apiUrl = "admin-api/RegistrationAdmin/org/#{orgId}/action/#{api}/#{token}/#{userName}"
#puts(apiUrl)

result = make_request(:post, apiUrl, param)
puts(result)
