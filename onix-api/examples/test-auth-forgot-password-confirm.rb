#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require 'cgi'
require 'base64'
require './utils'

$stdout.sync = true

################### Main #######################
load_env(".env")

url = "https://register-dev.please-scan.com/temp/forgot-password/0bd5abe8-55de-443c-a243-b97892b119ed?data=eyJFbWFpbCI6InBqYW1lLmZiQGdtYWlsLmNvbSIsIlVzZXJOYW1lIjoic2V1YnBvbmcubW9uIiwiUGFzc3dvcmQiOm51bGwsIk5hbWUiOm51bGwsIkxhc3RuYW1lIjpudWxsLCJJbnZpdGVkQnkiOm51bGwsIk9yZ1VzZXJJZCI6ImFmNzc4ZjNkLTYzMWItNGJmZC1iYWVmLTMwN2RhYjZjNWU2NiJ9"
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

api = "ConfirmForgotPasswordReset"

#puts(dataObj)
userName = dataObj['UserName']

param =  {
  Email: dataObj['Email'],
  UserName: "#{userName}",
  Password: "#{ENV['USER_PASSWORD']}",
  OrgUserId: dataObj['OrgUserId'],
}

ENV['API_KEY'] = nil # ถ้าไม่ใช้ API KEY ก็เซ็ตเป็น nil

apiUrl = "api/Registration/org/#{orgId}/action/#{api}/#{token}/#{userName}"
#puts(apiUrl)

result = make_request(:post, apiUrl, param)
puts(result)
