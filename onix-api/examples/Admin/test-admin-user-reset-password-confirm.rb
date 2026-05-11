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

url = "https://admin-dev.please-payment.com/forgot-password/global/62177983-fd33-478f-8392-f43f234c6364?data=eyJFbWFpbCI6ImhlbGxvLnNldWJAYWJjZGVmZy5jb20iLCJVc2VyTmFtZSI6InNldWJwb25nLnNvb2Rsb3IiLCJQYXNzd29yZCI6bnVsbCwiTmFtZSI6bnVsbCwiTGFzdG5hbWUiOm51bGwsIkludml0ZWRCeSI6bnVsbCwiT3JnVXNlcklkIjoiZ2xvYmFsIiwiT3JnVHlwZSI6bnVsbH0%3d"
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

api = "ConfirmForgotPasswordReset"

#puts(dataObj)
userName = dataObj['UserName']

param =  {
  Email: dataObj['Email'],
  UserName: "#{userName}",
  Password: "Abc12345$343#1",
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
