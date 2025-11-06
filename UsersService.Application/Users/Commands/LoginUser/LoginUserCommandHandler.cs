using MediatR;
using UsersService.Application.Interfaces;

namespace UsersService.Application.Users.Commands.LoginUser
{
    public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, string>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public LoginUserCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher, IJwtTokenGenerator jwtTokenGenerator)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _jwtTokenGenerator = jwtTokenGenerator;
        }
        public async Task<string> Handle(LoginUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);

            if (user == null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid credentials");

            var token = _jwtTokenGenerator.GenerateToken(user);

            return token;
        }
    }
}
